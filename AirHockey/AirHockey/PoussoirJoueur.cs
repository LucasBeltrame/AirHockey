using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Box2DX.Common;
using Box2DX.Dynamics;
using Box2DX.Collision;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Color = Windows.UI.Color;

namespace AirHockey
{
    /// <summary>
    /// Poussoir contrôlé par le joueur
    /// </summary>
    class PoussoirJoueur
    {
        private Body body;
        private int numJoueur;
        private float rayonPoussoir;
        private Vec2 lastPos;
        private bool manualMovement;
        private int score;
        private bool doUpdate;

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="world"></param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <param name="num"></param>
        /// <param name="rayon"></param>
        public PoussoirJoueur(World world, float posX, float posY, int num, float rayon = 40.0f)
        {
            doUpdate = false;
            numJoueur = num;
            lastPos = new Vec2(posX,posY);
            this.rayonPoussoir = rayon;
            manualMovement = true;
            score = 0;

            //Body
            BodyDef bd = new BodyDef();
            bd.MassData.I = 0.0f;
            bd.MassData.Mass = 10.0f;
            bd.Position.Set(posX, posY);
            body = world.CreateBody(bd);

            //Fixture
            CreateFixture();
        }

        /// <summary>
        /// Crée la fixture, qui assure la solidité de l'objet
        /// </summary>
        private void CreateFixture()
        {
            //Shape
            CircleDef cd = new CircleDef();
            cd.LocalPosition = Vec2.Zero;
            cd.Restitution = 1.0f;
            cd.Radius = rayonPoussoir;

            body.CreateFixture(cd);
        }

        /// <summary>
        /// Met à jour le joueur
        /// </summary>
        public void Update()
        {
            if (doUpdate)
            {
                body.DestroyFixture(body.GetFixtureList());
                CreateFixture();
                doUpdate = false;
            }

        }

        /// <summary>
        /// Dessine le poussoir
        /// </summary>
        /// <param name="canvas"></param>
        public void Draw(CanvasDrawingSession canvas)
        {
            ICanvasBrush poussoirBrush = new CanvasSolidColorBrush(canvas, Color.FromArgb(255, 146, 4, 14));
            canvas.FillEllipse(this.Pos.X, this.Pos.Y, this.Rayon, this.Rayon, poussoirBrush);
        }

        /// <summary>
        /// Applique une force au poussoir
        /// </summary>
        /// <param name="force"></param>
        public void ApplyForce(Vec2 force)
        {
            body.SetLinearVelocity(force);
        }

        /// <summary>
        /// Position du joueur
        /// </summary>
        public Vec2 Pos
        {
            get { return body.GetPosition(); }
            set { body.SetXForm(value, 0.0f); }
        }

        /// <summary>
        /// Dernière position avant la position actuelle
        /// </summary>
        public Vec2 LastPos
        {
            get { return lastPos; }
            set { lastPos.Set(value.X,value.Y); }
        }

        /// <summary>
        /// Rayon du poussoir
        /// </summary>
        public float Rayon
        {
            get
            {
                return rayonPoussoir;
            }
            set
            {
                rayonPoussoir = value;
                doUpdate = true;
            }
        }

        /// <summary>
        /// Retourne le contour du poussoir
        /// </summary>
        /// <returns></returns>
        public AABB getAABB()
        {
            AABB retour = new AABB();
            body.GetFixtureList().Shape.ComputeAABB(out retour,body.GetXForm());
            return retour;
        }

        /// <summary>
        /// Numéro du joueur
        /// </summary>
        public int NumJoueur
        {
            get
            {
                return numJoueur;
            }
        }

        /// <summary>
        /// Signale qu'on a déplacé le joueur manuellement
        /// </summary>
        public bool ManualMove
        {
            get {return manualMovement; }
            set { manualMovement = value; }
        }

        /// <summary>
        /// Score du joueur
        /// </summary>
        public int Score { get; set; }
    }
}
