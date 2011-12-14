using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Xna.Csg;
using Xna.Csg.Primitives;

namespace ShapeRenderer
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        float controlSpeed = 0.03f;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        BSP result;

        Vector2 rotation;
        BasicEffect effect;
        RasterizerState wireframeState;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            var a = new Cube();
            var b = new Icosahedron().Transform(Matrix.CreateScale(0.7f));

            result = BSP.Intersect(a, b);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            effect = new BasicEffect(GraphicsDevice);

            wireframeState = new RasterizerState()
            {
                FillMode = Microsoft.Xna.Framework.Graphics.FillMode.WireFrame,
                CullMode = CullMode.None,
            };
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.A))
                rotation.X += controlSpeed;
            if (Keyboard.GetState().IsKeyDown(Keys.D))
                rotation.X -= controlSpeed;
            if (Keyboard.GetState().IsKeyDown(Keys.W))
                rotation.Y += controlSpeed;
            if (Keyboard.GetState().IsKeyDown(Keys.S))
                rotation.Y -= controlSpeed;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            effect.World = Matrix.CreateScale(30) * Matrix.CreateRotationY(rotation.X) * Matrix.CreateRotationZ(rotation.Y);
            effect.View = Matrix.CreateLookAt(new Vector3(25, 20, 10), Vector3.Zero, Vector3.Up);
            effect.Projection = Matrix.CreatePerspectiveFieldOfView(2, GraphicsDevice.Viewport.AspectRatio, 1, 1000);
            effect.VertexColorEnabled = true;
            effect.TextureEnabled = false;

            GraphicsDevice.RasterizerState = wireframeState;

            DrawBspTree(result);

            base.Draw(gameTime);
        }

        private void DrawBspTree(BSP tree)
        {
            List<VertexPositionColor> vertices = new List<VertexPositionColor>();
            List<int> indices = new List<int>();
            tree.ToMesh<VertexPositionColor, int>(
                (p, n) => new VertexPositionColor(p, Color.Black),
                v => { vertices.Add(v); return vertices.Count - 1; },
                (a, b, c) =>
                {
                    indices.Add(a);
                    indices.Add(b);
                    indices.Add(c);
                }
            );

            DrawShape<VertexPositionColor>(effect, vertices.ToArray(), indices.ToArray());
        }

        private void DrawShape<V>(Effect e, V[] vertices, int[] indices) where V : struct, IVertexType
        {
            foreach (var item in e.Techniques)
            {
                foreach (var pass in item.Passes)
                {
                    pass.Apply();

                    GraphicsDevice.DrawUserIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        vertices,
                        0,
                        vertices.Length,
                        indices,
                        0,
                        indices.Length / 3
                    );
                }
            }
        }
    }
}
