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
    /// <summary>
    /// Gère la collision à l'aide de l'inteface ContactListener de BOX2DX
    /// </summary>
    class CollisionDetect : ContactListener
    {
        private int playerMarked;
        private bool goodCollideHappend;

        public CollisionDetect()
        {
            playerMarked = 0;
            goodCollideHappend = true;
        }

        /// <summary>
        /// Déclenché lors du début d'un contact entre deux objets
        /// </summary>
        /// <param name="contact"></param>
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

        /// <summary>
        /// Fonction vérifiant que ce sont les objets qui nous intéresse qui se touche
        /// </summary>
        /// <param name="Goal"></param>
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

        /// <summary>
        /// Fin du contact
        /// </summary>
        /// <param name="contact"></param>
        public void EndContact(Contact contact)
        {
            playerMarked = 0;
            if (goodCollideHappend)
            {
                goodCollideHappend = !goodCollideHappend;
                playerMarked = 0;
            }
        }

        /// <summary>
        /// Non implémentée
        /// </summary>
        /// <param name="contact"></param>
        /// <param name="impulse"></param>
        public void PostSolve(Contact contact, ContactImpulse impulse)
        {
            
        }

        /// <summary>
        /// Non implémentée
        /// </summary>
        /// <param name="contact"></param>
        /// <param name="oldManifold"></param>
        public void PreSolve(Contact contact, Manifold oldManifold)
        {
            
        }

        /// <summary>
        /// Retourne le joueur qui a marqué
        /// </summary>
        public int PlayerMarked
        {
            get { return playerMarked;}
        }
    }
}
