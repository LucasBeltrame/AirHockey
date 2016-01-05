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
        private const float RAYON_PALET = 10.0f;

        public Palet(World world, float posX, float posY)
        {

            BodyDef bd = new BodyDef();
            bd.MassData.I = 0.0f;
            bd.MassData.Mass = 1.0f;
            bd.Position.Set(posX, posY);
            bd.LinearDamping = 0.0f;
            body = world.CreateBody(bd);

            //Shape
            CircleDef cd = new CircleDef();
            //cd.Friction = 0.0f;
            cd.Restitution = 1.0f;
            cd.LocalPosition = Vec2.Zero;
            cd.Radius = RAYON_PALET;

            body.CreateFixture(cd);
            body.GetFixtureList().UserData = "PALET";

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
            //body.ApplyImpulse(impulse,Vec2.Zero);
            body.SetLinearVelocity(impulse);
        }

        public float Rayon
        {
            get
            {
                return RAYON_PALET;
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
