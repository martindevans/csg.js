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

        BSP shapeToRender;

        BasicEffect effect;
        RasterizerState wireframeState;
        RasterizerState solidState;
        Matrix projection;
        Matrix view;

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
            IsMouseVisible = true;

            shapeToRender = new Mesh(new[]
                {
                    new Vector3(-250, 0, -250),
                    new Vector3(250, 0, -250),
                    new Vector3(250, 0, 250),
                    new Vector3(-250, 0, 250),
                    new Vector3(-250, 500, -250),
                    new Vector3(250, 500, -250),
                    new Vector3(250, 500, 250),
                    new Vector3(-250, 500, 250)
                },
                new[] {
                    0, 1, 2, 0, 2, 3, //Bottom
                    4, 6, 5, 4, 7, 6, //Top
                    7, 2, 6, 7, 3, 2, //Front
                    6, 1, 5, 6, 2, 1, //Right
                    4, 0, 7, 7, 0, 3, //Right
                    4, 5, 1, 4, 1, 0, //Back
                },
                (i, v) => new Vertex(v, Vector3.Zero)
            );

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            effect = new BasicEffect(GraphicsDevice);

            view = Matrix.CreateLookAt(new Vector3(3, 3, 3), Vector3.Zero, Vector3.Up);
            projection = Matrix.CreatePerspectiveFieldOfView(2, GraphicsDevice.Viewport.AspectRatio, 1, 1000);

            wireframeState = new RasterizerState()
            {
                FillMode = FillMode.WireFrame,
                CullMode = CullMode.CullClockwiseFace,
            };
            solidState = new RasterizerState()
            {
                FillMode = FillMode.Solid,
                CullMode = CullMode.CullClockwiseFace,
            };
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            effect.View = view;
            effect.Projection = projection;
            effect.VertexColorEnabled = true;
            effect.TextureEnabled = false;

            GraphicsDevice.RasterizerState = wireframeState;
            GraphicsDevice.DepthStencilState = new DepthStencilState()
            {
                DepthBufferEnable = true,
                DepthBufferWriteEnable = true,
            };

            float time = (float)gameTime.TotalGameTime.TotalSeconds / 1.5f;
            DrawShape(effect, shapeToRender, Matrix.CreateScale(0.005f) * Matrix.CreateRotationY(time) * Matrix.CreateRotationX(time) * Matrix.CreateTranslation(0, -0.5f, 0), Color.Green);

            base.Draw(gameTime);
        }

        private void DrawShape(BasicEffect e, BSP bsp, Matrix transform, Color color)
        {
            List<VertexPositionColor> vertices = new List<VertexPositionColor>();
            List<int> indices = new List<int>();

            bsp.ToTriangleList<VertexPositionColor, int>(
                v => new VertexPositionColor(v.Position, v is ColorVertex ? ((ColorVertex)v).Color : Color.White),
                v =>
                {
                    vertices.Add(v);
                    return vertices.Count - 1;
                },
                (a, b, c) =>
                {
                    indices.Add(a);
                    indices.Add(b);
                    indices.Add(c);
                }
            );

            DrawShape<VertexPositionColor>(effect, vertices.ToArray(), indices.ToArray(), transform);
        }

        private void DrawShape<V>(BasicEffect e, V[] vertices, int[] indices, Matrix transform) where V : struct, IVertexType
        {
            e.World = transform;
            //e.EnableDefaultLighting();

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
