using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using Box2DX;
using Box2DX.Collision;
using Box2DX.Common;
using Box2DX.Dynamics;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Color = Windows.UI.Color;

// Pour plus d'informations sur le modèle d'élément Page vierge, voir la page http://go.microsoft.com/fwlink/?LinkId=234238

namespace AirHockey
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class GamePage : Page
    {
        //Solution nulle
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

        //Attributs liés aux joueurs
        private PoussoirJoueur joueur1;
        private PoussoirJoueur joueur2;
        private Vec2 newP1Pos;
        private Vec2 newP2Pos;

        //Attributs liés au jeu de manière généale
        private Palet palet;
        private Vec2 paletInitalPos;
        private Body[] borduresBodies;



        /// <summary>
        /// Constructeur
        /// </summary>
        public GamePage()
        {
            this.InitializeComponent();
            width = 400.0f;
            height = 800.0f;

            InitializePhysics();
            InitialiseBordure();
            //Joueurs
            joueur1 = new PoussoirJoueur(world,width/2, 0,1);
            joueur2 = new PoussoirJoueur(world, width / 2, 100, 2);
            newP1Pos = joueur1.Pos;
            newP2Pos = joueur2.Pos;

            //Palet
            palet = new Palet(world,width/2,height/2);
            paletInitalPos = palet.Pos;

            //Tâche
            executionAction = GameExecution;
            executionTask = new Task(executionAction);
            executionTask.Start();
            
        }
        
        /// <summary>
        /// Dessin du jeu dans le canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void CanvasControl_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
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

            /* Palet */
            palet.Draw(args.DrawingSession);

            /* Joueurs */
            joueur1.Draw(args.DrawingSession);
            joueur2.Draw(args.DrawingSession);
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

        private void InitialiseBordure()
        {
            Vec2[] vertices = new Vec2[4];
            borduresBodies = new Body[4];

            //Création des bordures du monde
            BodyDef boundaryDef = new BodyDef();
            boundaryDef.MassData.I = 0.0f;
            boundaryDef.MassData.Mass = 0.0f;

            //Haut
            InitialisationBordure(borduresBodies[0],width/2,0.0f,width,1.0f);

            //Droite
            InitialisationBordure(borduresBodies[1], width, height/2, 1.0f, height);

            //Bas
            InitialisationBordure(borduresBodies[2], width / 2,height, width, 1.0f);

            //Gauche
            InitialisationBordure(borduresBodies[3], 0.0f, height/2, 1.0f, height);
            
        }

        private void InitialisationBordure(Body body, float posx, float posy, float boxWidth, float boxHeight)
        {
            BodyDef boundaryDef = new BodyDef();
            boundaryDef.MassData.I = 0.0f;
            boundaryDef.MassData.Mass = 0.0f;
            boundaryDef.Position.Set(posx, posy);
            body = world.CreateBody(boundaryDef);

            PolygonDef pd = new PolygonDef();
            pd.SetAsBox(boxWidth, boxHeight);
            pd.Restitution = 1.0f;
            body.CreateFixture(pd);
        }

        /// <summary>
        /// Replace les joueurs au centre de leur partie du terrain et remet le palet au centre
        /// </summary>
        private void ResetGame()
        {
            Vec2 posJ1 = new Vec2(width/2, height - joueur1.Rayon/2);
            Vec2 posJ2 = new Vec2(width/2, joueur2.Rayon/2);

            newP1Pos = posJ1;
            newP2Pos = posJ2;
            palet.Pos = paletInitalPos;
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
        private void UpdateGame()
        {
            joueur1.Pos = newP1Pos;
            joueur2.Pos = newP2Pos;
            world.Step(1.0f / 6.0f, 1, 1);
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
            double tempX = e.GetCurrentPoint(this).Position.X;
            double tempY = e.GetCurrentPoint(this).Position.Y;
            if (tempY > this.ActualHeight / 2)
            {
                DeplacementPoussoir(joueur1,e,joueur1.NumJoueur);
            }
            else
            {
                DeplacementPoussoir(joueur2, e, joueur2.NumJoueur);
            }
        }

        /// <summary>
        /// Gère le déplcement des poussoirs en fonction du mouvement des doigts
        /// </summary>
        /// <param name="joueur"></param>
        /// <param name="e"></param>
        /// <param name="numJoueur"></param>
        private void DeplacementPoussoir(PoussoirJoueur joueur,PointerRoutedEventArgs e,int numJoueur)
        {
            float tempX = (float)e.GetCurrentPoint(this).Position.X;
            float tempY = (float)e.GetCurrentPoint(this).Position.Y;
            AABB joueurAABB = joueur.getAABB();

            if (tempX >= joueurAABB.LowerBound.X && tempX <= joueurAABB.UpperBound.X)
            {
                if (tempY >= joueurAABB.LowerBound.Y && tempY <= joueurAABB.UpperBound.Y)
                {
                    System.Diagnostics.Debug.WriteLine("T'es sur un joueur mec");
                    switch (numJoueur)
                    {
                        case 1:
                            newP1Pos = new Vec2(tempX, tempY);
                            break;
                        case 2:
                            newP2Pos = new Vec2(tempX, tempY);
                            break;
                    }
                }
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
            
        }
    }
}
