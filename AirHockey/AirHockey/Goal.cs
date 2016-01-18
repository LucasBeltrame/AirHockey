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
    class Goal
    {
        private Body body;
        private int numPlayer;
        private float largeurGoal;
        private const float HAUTEUR_GOAL = 4.0f;
        private bool doUpdate;

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

        private void CreateFixture()
        {
            //Shape
            PolygonDef pd = new PolygonDef();
            pd.SetAsBox(largeurGoal / 2, HAUTEUR_GOAL / 2);
            pd.Restitution = 1.0f;
            body.CreateFixture(pd);
            body.GetFixtureList().UserData = "GOALP" + numPlayer.ToString();
        }

        public void Update()
        {
            if (doUpdate)
            {
                body.DestroyFixture(body.GetFixtureList());
                CreateFixture();
                doUpdate = false;
            }
        }

        public Vec2 Pos
        {
            get { return body.GetPosition(); }
            set { body.SetXForm(value, 0.0f); }
        }

        public float LargeurGoal
        {
            get {return largeurGoal; }
            set
            {
                largeurGoal = value;
                doUpdate = true;
            }
        }

        public void Draw(CanvasDrawingSession canvas)
        {
            ICanvasBrush goalBrush = new CanvasSolidColorBrush(canvas, Color.FromArgb(255,255,0,0));
            canvas.FillRectangle(this.Pos.X - largeurGoal/2,this.Pos.Y - HAUTEUR_GOAL/2, largeurGoal,HAUTEUR_GOAL, goalBrush);
        }
    }
}
