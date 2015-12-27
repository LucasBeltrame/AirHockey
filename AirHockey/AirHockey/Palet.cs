using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Box2DX.Collision;
using Box2DX.Common;
using Box2DX.Dynamics;

namespace AirHockey
{
    class Palet
    {
        private Body body;
        private const float RAYON_PALET = 10.0f;

        public Palet(World world, float posX, float posY)
        {

            BodyDef bd = new BodyDef();
            bd.MassData.I = 1.0f;
            bd.MassData.Mass = 1.0f;
            bd.Position.Set(posX, posY);
            body = world.CreateBody(bd);
        }

        public Vec2 Pos
        {
            get { return body.GetPosition(); }
            set { body.SetXForm(value, 0.0f); }
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
