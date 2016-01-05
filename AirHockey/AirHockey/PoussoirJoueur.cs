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
    class PoussoirJoueur
    {
        private Body body;
        private int numJoueur;
        private const float RAYON_POUSSOIR = 50.0f;
        private Vec2 lastPos;
        private bool manualMovement;
        private int score;


        public PoussoirJoueur(World world, float posX, float posY, int num)
        {
            numJoueur = num;
            lastPos = new Vec2(posX,posY);
            manualMovement = true;
            score = 0;

            //Body
            BodyDef bd = new BodyDef();
            bd.MassData.I = 0.0f;
            bd.MassData.Mass = 10.0f;
            bd.Position.Set(posX, posY);
            body = world.CreateBody(bd);

            //Shape
            CircleDef cd = new CircleDef();
            cd.LocalPosition = Vec2.Zero;
            cd.Restitution = 1.0f;
            cd.Radius = RAYON_POUSSOIR;

            body.CreateFixture(cd);
        }

        public void Draw(CanvasDrawingSession canvas)
        {
            ICanvasBrush poussoirBrush = new CanvasSolidColorBrush(canvas, Color.FromArgb(255,127,127,0));
            canvas.FillEllipse(this.Pos.X, this.Pos.Y, this.Rayon, this.Rayon, poussoirBrush);
        }

        public void ApplyForce(Vec2 force)
        {
            body.SetLinearVelocity(force);
            //body.ApplyImpulse(force,Vec2.Zero);
        }

        public Vec2 Pos
        {
            get { return body.GetPosition(); }
            set { body.SetXForm(value, 0.0f); }
        }

        public Vec2 LastPos
        {
            get { return lastPos; }
            set { lastPos.Set(value.X,value.Y); }
        }

        public float Rayon
        {
            get
            {
                return RAYON_POUSSOIR;
            }
        }

        public AABB getAABB()
        {
            AABB retour = new AABB();
            body.GetFixtureList().Shape.ComputeAABB(out retour,body.GetXForm());
            return retour;
        }

        public int NumJoueur
        {
            get
            {
                return numJoueur;
            }
        }

        public bool ManualMove
        {
            get {return manualMovement; }
            set { manualMovement = value; }
        }

        public int Score { get; set; }
    }
}
