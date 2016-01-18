using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Box2DX.Collision;
using Box2DX.Common;
using Box2DX.Dynamics;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Color = Windows.UI.Color;
using System.Numerics;
using Windows.Foundation;
using Microsoft.Graphics.Canvas.Text;

// Pour plus d'informations sur le modèle d'élément Page vierge, voir la page http://go.microsoft.com/fwlink/?LinkId=234238

namespace AirHockey
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class GamePage : Page
    {
        //Empêche un bug
        private bool firstDraw = true;

        //Hauteur et largeur de la fenêtre
        private float width;
        private float height;

        //Attributs liés à la tâche
        private Action executionAction;
        private Task executionTask;
        private bool isRunning = true;

        //Attributs liés à la partie physique du jeu
        private World world;
        private CollisionDetect collisionDetect;

        //Attributs liés aux joueurs
        private PoussoirJoueur joueur1;
        private PoussoirJoueur joueur2;
        private Vec2 newP1Pos;
        private Vec2 newP2Pos;
        private Vec2 lastMousePosJ1;
        private Vec2 lastMousePosJ2;

        //Attributs liés aux goals
        private Goal goalP1;
        private Goal goalP2;

        //Attributs liés au jeu de manière généale
        private Palet palet;
        private Vec2 paletInitalPos;
        private Body[] borduresBodies;
        private bool isResized;
        private CanvasBitmap imageHeArc;

        //Constantes
        private const float FACTEUR_SPEED = 5.0f;
        private const float FACTEUR_SIZE_JOUEUR = 2.0f/25.0f;
        private const float FACTEUR_SIZE_PALET = 2.0f / 25.0f;
        private const float FACTEUR_SIZE_GOAL = 2.0f / 25.0f;



        /// <summary>
        /// Constructeur
        /// </summary>
        public GamePage()
        {
            this.InitializeComponent();
            //On charge l'image une fois
            imageHeArc = null;
            width = 400.0f;
            height = 800.0f;

            isResized = true;
            borduresBodies = new Body[4];

            InitializePhysics();
            InitialiseBordure();
            //Joueurs
            joueur1 = new PoussoirJoueur(world,width/2, 0,1);
            joueur2 = new PoussoirJoueur(world, width / 2, 200, 2);
            newP1Pos = joueur1.Pos;
            newP2Pos = joueur2.Pos;
            lastMousePosJ1 = new Vec2(0.0f,0.0f);
            lastMousePosJ2 = new Vec2(0.0f, 0.0f);

            //Palet
            palet = new Palet(world,width/2,height/2);
            paletInitalPos = palet.Pos;

            //Goal
            goalP1 = new Goal(world,width/2,1.0f,1);
            goalP2 = new Goal(world, width / 2,200, 2);

            //Détection de collision
            collisionDetect = new CollisionDetect();
            world.SetContactListener(collisionDetect);

            //Tâche
            executionAction = GameExecution;
            executionTask = new Task(executionAction);
            executionTask.Start();
            
        }

        private async void loadImage(CanvasControl canvas)
        {
            imageHeArc = await CanvasBitmap.LoadAsync(canvas, "Assets/logo.png");
        }
        
        /// <summary>
        /// Dessin du jeu dans le canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void CanvasControl_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
//            if (imageHeArc == null)
//            {
//                loadImage(sender);
//            }

            /* TERRAIN */
            //Ligne centrale
            args.DrawingSession.DrawLine(0.0f,height/2,width,height/2,Color.FromArgb(255,0,0,0));
            //Cercle central
            float radiusCentralCircle = 0.0f;
            if (this.ActualWidth < this.ActualHeight)
            {
                radiusCentralCircle = width/4;
            }
            else
            {
                radiusCentralCircle = height/4;
            }
            args.DrawingSession.DrawEllipse(width/2, height/2,radiusCentralCircle, radiusCentralCircle, Color.FromArgb(255,255,0,0));
           
            //Logo
//            if (imageHeArc != null)
//            {
//                Rect finalRect = new Rect(width/2.0f - radiusCentralCircle, height/2.0f - imageHeArc.Bounds.Height/4.0f, 2 * radiusCentralCircle, (2 * radiusCentralCircle) / (imageHeArc.Bounds.Width / imageHeArc.Bounds.Height));
//                args.DrawingSession.DrawImage(imageHeArc, finalRect, imageHeArc.Bounds,0.5f);
//            }

            //Sera vilain sur un écran mis en position landscape ! 
            //Demi Cercle P1
            args.DrawingSession.DrawEllipse(width / 2, height, goalP1.LargeurGoal * 0.75f, goalP1.LargeurGoal * 0.75f, Color.FromArgb(255, 0, 0, 0));
            //Demi Cercle P2
            args.DrawingSession.DrawEllipse(width / 2, 0.0f, goalP1.LargeurGoal * 0.75f, goalP1.LargeurGoal * 0.75f, Color.FromArgb(255, 0, 0, 0));

            /* Score */
            float scoreJ1PosX = (width/10)*1;
            float scoreJ1PosY = (height/10)*9;

            float scoreJ2PosX = (width / 10) * 1 - width;
            float scoreJ2PosY = (height / 10) * 9 - height;

            //Font and stuff
            CanvasTextFormat ctf = new CanvasTextFormat();
            ctf.FontFamily = "Segoe MDL2";
            ctf.FontSize = 60.0f;
            Color textColor = Color.FromArgb(150, 0, 0, 0);

            args.DrawingSession.DrawText(joueur1.Score.ToString(), scoreJ1PosX, scoreJ1PosY, textColor, ctf);
            // ReSharper disable All
            args.DrawingSession.Transform = Matrix3x2.CreateRotation((float)System.Math.PI);
            args.DrawingSession.DrawText(joueur2.Score.ToString(), scoreJ2PosX, scoreJ2PosY, textColor, ctf);
            args.DrawingSession.Transform = Matrix3x2.Identity;

            /* Palet */
            palet.Draw(args.DrawingSession);

            /* Joueurs */
            joueur1.Draw(args.DrawingSession);
            joueur2.Draw(args.DrawingSession);

            /* Goal */
            goalP1.Draw(args.DrawingSession);
            goalP2.Draw(args.DrawingSession);


            /*debug*/
            for (int i = 0; i < borduresBodies.Length; i++)
            {
                DrawWallDebug(args.DrawingSession, i);
            }
            
        }

        private void DrawWallDebug(CanvasDrawingSession canvas, int index)
        {
            try
            {
                if (!isResized)
                {
                    AABB aabb = new AABB();
                    ICanvasBrush brush = new CanvasSolidColorBrush(canvas, Color.FromArgb(255, 127, 127, 0));
                    borduresBodies[index].GetFixtureList().Shape.ComputeAABB(out aabb, borduresBodies[index].GetXForm());
                    canvas.FillRectangle(
                        aabb.LowerBound.X, aabb.LowerBound.Y, aabb.UpperBound.X - aabb.LowerBound.X,
                        aabb.UpperBound.Y - aabb.LowerBound.Y, brush
                        );
                }
            }
            catch (Exception)
            {
                
            }
            
        }

        /// <summary>
        /// Regroupe les initialisations liées à Box2DX
        /// </summary>
        private void InitializePhysics()
        {
            //Création du monde
            AABB worldAABB = new AABB();
            worldAABB.LowerBound = new Vec2(-100.0f,-100.0f);
            worldAABB.UpperBound = new Vec2(8000.0f,8000.0f);
            world = new World(worldAABB, new Vec2(0.0f,0.0f), true);
        }

        /// <summary>
        /// Lance la procédure d'initialisation des bordures du jeu
        /// </summary>
        private void InitialiseBordure()
        {
            Vec2[] vertices = new Vec2[4];
            foreach (Body b in borduresBodies)
            {
                if (b != null)
                {
                    world.DestroyBody(b);
                }
            }
            borduresBodies = new Body[4];

            //Création des bordures du monde
            BodyDef boundaryDef = new BodyDef();
            boundaryDef.MassData.I = 0.0f;
            boundaryDef.MassData.Mass = 0.0f;

            //Haut
            InitialisationBordure(ref borduresBodies[0],width/2,0.0f,width,1.0f);

            //Droite
            InitialisationBordure(ref borduresBodies[1], width, height/2, 1.0f, height);

            //Bas
            InitialisationBordure(ref borduresBodies[2], width / 2,height, width, 1.0f);

            //Gauche
            InitialisationBordure(ref borduresBodies[3], 0.0f, height/2, 1.0f, height);
            
        }

        /// <summary>
        /// Initialise une bordure du jeu
        /// </summary>
        /// <param name="body"></param>
        /// <param name="posx"></param>
        /// <param name="posy"></param>
        /// <param name="boxWidth"></param>
        /// <param name="boxHeight"></param>
        private void InitialisationBordure(ref Body body, float posx, float posy, float boxWidth, float boxHeight)
        {
            BodyDef boundaryDef = new BodyDef();
            boundaryDef.MassData.I = 0.0f;
            boundaryDef.MassData.Mass = 0.0f;
            boundaryDef.Position.Set(posx, posy);
            body = world.CreateBody(boundaryDef);

            PolygonDef pd = new PolygonDef();
            pd.SetAsBox(boxWidth, boxHeight);
            pd.Restitution = 0.9f;
            body.CreateFixture(pd);
        }

        /// <summary>
        /// Replace les joueurs au centre de leur partie du terrain et remet le palet au centre
        /// </summary>
        private void ResetGame()
        {
            Vec2 posJ1 = new Vec2(width/2, height - (joueur1.Rayon * 2));
            Vec2 posJ2 = new Vec2(width/2, joueur2.Rayon*2);

            newP1Pos = posJ1;
            newP2Pos = posJ2;
            joueur1.ManualMove = true;
            joueur2.ManualMove = true;
            palet.Pos = paletInitalPos;

            palet.ApplyImpulse(getRandomImpulse());
        }

        private Vec2 getRandomImpulse()
        {
            Random r = new Random();
            float signeX = 1.0f;
            float signeY = 1.0f;

            if (r.Next(2) == 1)
            {
                signeX *= -1;
            }

            if (r.Next(2) == 1)
            {
                signeY *= -1;
            }

            return new Vec2((float)r.NextDouble() * FACTEUR_SPEED* signeX, (float)r.NextDouble() * FACTEUR_SPEED * signeY);
        }

        /// <summary>
        /// La tâche qui va tourner en boucle et qui va mettre à jour le jeu puis dessiner ce dernier
        /// </summary>
        private void GameExecution()
        {
            while (isRunning)
            {
                if (firstDraw)
                {
                    firstDraw = !firstDraw;
                    ResetGame();
                }
                UpdateGame();
                DrawGame();
                Task.Delay(16);
            }
        }

        /// <summary>
        /// Mise à jour de la logique du jeu
        /// </summary>
        private async Task UpdateGame()
        {
            bool doPause = false;

            //Update joueur
            joueur1.Update();
            joueur2.Update();

            //Update goal
            goalP1.Update();
            goalP2.Update();

            //Update Palet
            palet.Update();

            checkPlayersInBound();
            if (collisionDetect.PlayerMarked != 0)
            {
                switch (collisionDetect.PlayerMarked)
                {
                    case 1:
                        joueur2.Score++;
                        break;
                    case 2:
                        joueur1.Score++;
                        break;
                }
                ResetGame();
                doPause = true;

            }
            if (palet.Pos.X < 0 || palet.Pos.X > width || palet.Pos.Y < 0 || palet.Pos.Y > height)
            {
                palet.Pos = paletInitalPos;
            }

            if (joueur1.ManualMove)
            {
                joueur1.Pos = newP1Pos;
                joueur1.ManualMove = !joueur1.ManualMove;
            }
            if (joueur2.ManualMove)
            {
                joueur2.Pos = newP2Pos;
                joueur2.ManualMove = !joueur2.ManualMove;
            }

            if (isResized)
            {
                isResized = !isResized;
                goalP1.Pos = new Vec2(width / 2, 1.0f);
                goalP2.Pos = new Vec2(width / 2, height - 1.0f);
                InitialiseBordure();
            }
            if (doPause)
            {
                Task.Delay(3000).Wait();
            }
            //1.0f/60.0f
            world.Step(1.0f / 500.0f, 1, 1);
            joueur1.ApplyForce(Vec2.Zero);
            joueur2.ApplyForce(Vec2.Zero);
        }

        /// <summary>
        /// Regarde si le joueur est encore sur l'écran. Si ce n'est pas le cas on le replace à sa position par défaut
        /// </summary>
        private void checkPlayersInBound()
        {
            if (joueur1.Pos.X < 0 || joueur1.Pos.X > width)
            {
                Vec2 posJ1 = new Vec2(width / 2, height - (joueur1.Rayon * 2));
                newP1Pos = posJ1;
                joueur1.ManualMove = true;
            }
            else if (joueur1.Pos.Y < 0 || joueur1.Pos.Y > height)
            {
                Vec2 posJ1 = new Vec2(width / 2, height - (joueur1.Rayon * 2));
                newP1Pos = posJ1;
                joueur1.ManualMove = true;
            }

            if (joueur2.Pos.X < 0 || joueur2.Pos.X > width)
            {
                Vec2 posJ2 = new Vec2(width / 2, joueur2.Rayon * 2);
                newP2Pos = posJ2;
                joueur2.ManualMove = true;
            }
            else if (joueur2.Pos.Y < 0 || joueur2.Pos.Y > height)
            {
                Vec2 posJ2 = new Vec2(width / 2, joueur2.Rayon * 2);
                newP2Pos = posJ2;
                joueur2.ManualMove = true;
            }
        }

        /// <summary>
        /// Fonction qui invalide le canvas et force son redessinage
        /// </summary>
        private void InvalidateCanvas()
        {
            canvasDessin.Invalidate();
        }


        /// <summary>
        /// Dessin du jeu mis à jour
        /// </summary>
        async private void DrawGame()
        {
            try
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { InvalidateCanvas(); });
                
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                throw;
            }
        }

        /// <summary>
        /// Événement qui dessine l'interface de jeu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanvasDessin_OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            bool isMouse = false;
            Windows.UI.Xaml.Input.Pointer ptr = e.Pointer;
            Windows.UI.Input.PointerPoint ptrPt = e.GetCurrentPoint(this.canvasDessin);

            if (ptr.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                isMouse = true;
            }

            if (!isMouse || ptrPt.Properties.IsLeftButtonPressed)
            {
                double tempX = e.GetCurrentPoint(this).Position.X;
                double tempY = e.GetCurrentPoint(this).Position.Y;
                if (tempY > this.ActualHeight/2)
                {
                    DeplacementPoussoir(joueur1, e, sender, joueur1.NumJoueur, lastMousePosJ1);
                    lastMousePosJ1.Set((float) tempX, (float) tempY);
                }
                else
                {
                    DeplacementPoussoir(joueur2, e, sender, joueur2.NumJoueur, lastMousePosJ2);
                    lastMousePosJ2.Set((float) tempX, (float) tempY);
                }
            }
        }

        /// <summary>
        /// Gère le déplcement des poussoirs en fonction du mouvement des doigts
        /// </summary>
        /// <param name="joueur"></param>
        /// <param name="e"></param>
        /// <param name="numJoueur"></param>
        private void DeplacementPoussoir(PoussoirJoueur joueur,PointerRoutedEventArgs e, object sender,int numJoueur, Vec2 lastMousePos)
        {
            float posX = (float)e.GetCurrentPoint(this.canvasDessin).Position.X;
            float posY = (float)e.GetCurrentPoint(this.canvasDessin).Position.Y;
            bool notInPlayer = true;


            AABB joueurAABB = joueur.getAABB();
            
            if (posX >= joueurAABB.LowerBound.X && posX <= joueurAABB.UpperBound.X)
            {
                if (posY >= joueurAABB.LowerBound.Y && posY <= joueurAABB.UpperBound.Y)
                {
                    notInPlayer = false;
                    Vec2 force = new Vec2(posX - lastMousePos.X, posY - lastMousePos.Y);
                    force*=0.5f;
                    joueur.ApplyForce(force);
                    joueur.LastPos = new Vec2(posX,posY);
                    joueur.ManualMove = true;
                    
                    switch (numJoueur)
                    {
                        case 1:
                            //Ancienne méthode
                            newP1Pos = new Vec2(posX, posY);
                            break;
                        case 2:
                            //Ancienne méthode
                            newP2Pos = new Vec2(posX, posY);
                            break;
                    }
                    
                }
            }

            if (notInPlayer)
            {
                joueur.ApplyForce(Vec2.Zero);
            }

            
        }

        /// <summary>
        /// Événement qui adapte l'interface en fonction de la taille de la fenêtre
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanvasDessin_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.width = (float) this.ActualWidth;
            this.height = (float) this.ActualHeight;
            
            paletInitalPos = new Vec2(width / 2, height / 2);

            //On update la taille des joueurs en fonction de la width
            joueur1.Rayon = width*FACTEUR_SIZE_JOUEUR;
            joueur2.Rayon = width*FACTEUR_SIZE_JOUEUR;
            goalP1.LargeurGoal = (width/100.0f)*30.0f;
            goalP2.LargeurGoal = (width / 100.0f) * 30.0f;

            //Défini au cas où l'application ne serait pas en mode portrait
            if (width > height)
            {
                joueur1.Rayon = height*FACTEUR_SIZE_JOUEUR;
                joueur2.Rayon = height*FACTEUR_SIZE_JOUEUR;
            }

            //On défini la taille du palet en fonction de la taille des joueurs
            palet.Rayon = joueur1.Rayon/2.0f;

            //Mise à jour des bordures
            isResized = true;
            System.Diagnostics.Debug.WriteLine(width);

        }

        /// <summary>
        /// Gère l'événement de pression (click, touch etc.)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanvasDessin_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            double tempX = e.GetCurrentPoint(this).Position.X;
            double tempY = e.GetCurrentPoint(this).Position.Y;
            if (tempY > this.ActualHeight / 2)
            {
                lastMousePosJ1.Set((float)tempX, (float)tempY);
            }
            else
            {
                lastMousePosJ2.Set((float)tempX, (float)tempY);
            }
        }
    }
}
