using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Box2DX.Collision;
using Box2DX.Common;
using Box2DX.Dynamics;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Color = Windows.UI.Color;

namespace AirHockey
{
    /// <summary>
    /// But sur le terrain
    /// </summary>
    class Goal
    {
        private Body body;
        private int numPlayer;
        private float largeurGoal;
        private const float HAUTEUR_GOAL = 4.0f;
        private bool doUpdate;

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="world"></param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <param name="num"></param>
        /// <param name="largeur"></param>
        public Goal(World world, float posX, float posY, int num, float largeur = 150.0f)
        {
            numPlayer = num;
            largeurGoal = largeur;
            doUpdate = false;

            BodyDef bd = new BodyDef();
            bd.MassData.I = 0.0f;
            bd.MassData.Mass = 0.0f;
            bd.Position.Set(posX, posY);
            bd.LinearDamping = 0.0f;
            body = world.CreateBody(bd);
            CreateFixture();

        }

        /// <summary>
        /// Crée la fixture, qui assure la solidité de l'objet
        /// </summary>
        private void CreateFixture()
        {
            //Shape
            PolygonDef pd = new PolygonDef();
            pd.SetAsBox(largeurGoal / 2, HAUTEUR_GOAL / 2);
            pd.Restitution = 1.0f;
            body.CreateFixture(pd);
            body.GetFixtureList().UserData = "GOALP" + numPlayer.ToString();
        }

        /// <summary>
        /// Met à jour le but
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
        /// Position du but
        /// </summary>
        public Vec2 Pos
        {
            get { return body.GetPosition(); }
            set { body.SetXForm(value, 0.0f); }
        }

        /// <summary>
        /// Largeur du but
        /// </summary>
        public float LargeurGoal
        {
            get {return largeurGoal; }
            set
            {
                largeurGoal = value;
                doUpdate = true;
            }
        }

        /// <summary>
        /// Dessine le but
        /// </summary>
        /// <param name="canvas"></param>
        public void Draw(CanvasDrawingSession canvas)
        {
            ICanvasBrush goalBrush = new CanvasSolidColorBrush(canvas, Color.FromArgb(255,255,0,0));
            canvas.FillRectangle(this.Pos.X - largeurGoal/2,this.Pos.Y - HAUTEUR_GOAL/2, largeurGoal,HAUTEUR_GOAL, goalBrush);
        }
    }
}
