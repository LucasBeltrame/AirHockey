using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Box2DX.Collision;
using Box2DX.Dynamics;

namespace AirHockey
{
    class CollisionDetect : ContactListener
    {
        private int playerMarked;
        private bool goodCollideHappend;

        public CollisionDetect()
        {
            playerMarked = 0;
            goodCollideHappend = true;
        }

        public void BeginContact(Contact contact)
        {
            if (contact.FixtureA.UserData != null && contact.FixtureB.UserData != null)
            {
                String fix1 = (String) contact.FixtureA.UserData;
                String fix2 = (String)contact.FixtureB.UserData;

                if (fix1.Equals("PALET"))
                {
                    CheckCollide(fix2);
                }
                else if (fix2.Equals("PALET"))
                {
                    CheckCollide(fix1);
                }
            }
            
        }

        private void CheckCollide(String Goal)
        {
            if (Goal.Equals("GOALP1"))
            {
                playerMarked = 2;
                goodCollideHappend = true;
            }
            else if (Goal.Equals("GOALP2"))
            {
                playerMarked = 1;
                goodCollideHappend = true;
            }
        }

        public void EndContact(Contact contact)
        {
            playerMarked = 0;
            if (goodCollideHappend)
            {
                goodCollideHappend = !goodCollideHappend;
                playerMarked = 0;
            }
        }

        public void PostSolve(Contact contact, ContactImpulse impulse)
        {
            
        }

        public void PreSolve(Contact contact, Manifold oldManifold)
        {
            
        }

        public int PlayerMarked
        {
            get { return playerMarked;}
        }
    }
}
