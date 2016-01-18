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
    class Palet
    {
        private Body body;
        private float rayonPalet;
        private bool doUpdate;

        public Palet(World world, float posX, float posY, float rayon = 15.0f)
        {
            rayonPalet = rayon;
            BodyDef bd = new BodyDef();
            bd.MassData.I = 0.0f;
            bd.MassData.Mass = 1.0f;
            bd.Position.Set(posX, posY);
            bd.LinearDamping = 0.0f;
            body = world.CreateBody(bd);
            CreateFixture();

        }

        private void CreateFixture()
        {
            //Shape
            CircleDef cd = new CircleDef();
            //cd.Friction = 0.0f;
            cd.Restitution = 1.0f;
            cd.LocalPosition = Vec2.Zero;
            cd.Radius = rayonPalet;

            body.CreateFixture(cd);
            body.GetFixtureList().UserData = "PALET";
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

        public void Draw(CanvasDrawingSession canvas)
        {
            ICanvasBrush paletBrush = new CanvasSolidColorBrush(canvas, Color.FromArgb(255, 5, 5, 5));
            canvas.FillEllipse(this.Pos.X, this.Pos.Y, this.Rayon, this.Rayon, paletBrush);
        }

        public void ApplyImpulse(Vec2 impulse)
        {
            body.SetLinearVelocity(impulse);
        }

        public float Rayon
        {
            get
            {
                return rayonPalet;
            }
            set
            {
                rayonPalet = value;
                doUpdate = true;
            }
        }

        public AABB getAABB()
        {
            AABB retour = new AABB();
            body.GetFixtureList().Shape.ComputeAABB(out retour, body.GetXForm());
            return retour;
        }

    }
}
