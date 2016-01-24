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
    /// Représente le palet de jeu
    /// </summary>
    class Palet
    {
        private Body body;
        private float rayonPalet;
        private bool doUpdate;

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="world"></param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <param name="rayon"></param>
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

        /// <summary>
        /// Crée la fixture, qui assure la solidité de l'objet
        /// </summary>
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

        /// <summary>
        /// Met à jour le palet
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
        /// Position du palet
        /// </summary>
        public Vec2 Pos
        {
            get { return body.GetPosition(); }
            set { body.SetXForm(value, 0.0f); }
        }

        /// <summary>
        /// Dessine le palet
        /// </summary>
        /// <param name="canvas"></param>
        public void Draw(CanvasDrawingSession canvas)
        {
            ICanvasBrush paletBrush = new CanvasSolidColorBrush(canvas, Color.FromArgb(255, 5, 5, 5));
            canvas.FillEllipse(this.Pos.X, this.Pos.Y, this.Rayon, this.Rayon, paletBrush);
        }

        /// <summary>
        /// Applique une impulsion au palet
        /// </summary>
        /// <param name="impulse"></param>
        public void ApplyImpulse(Vec2 impulse)
        {
            body.SetLinearVelocity(impulse);
        }

        /// <summary>
        /// Rayon du palet
        /// </summary>
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

        /// <summary>
        /// Retourne le contour du palet
        /// </summary>
        /// <returns></returns>
        public AABB getAABB()
        {
            AABB retour = new AABB();
            body.GetFixtureList().Shape.ComputeAABB(out retour, body.GetXForm());
            return retour;
        }

    }
}
