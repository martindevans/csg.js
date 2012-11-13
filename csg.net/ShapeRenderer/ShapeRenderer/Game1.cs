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

        private BSP[] shapes;
        BSP shapeToRender;

        Vector2 rotation;
        BasicEffect effect;
        RasterizerState wireframeState;
        RasterizerState solidState;
        Matrix projection;
        Matrix view;

        BSP sphere;

        Vector3? mouseIntersectionPosition;

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

            const float scale = 0.3f;

            var a = new Cylinder(10, (p, n) => new ColorVertex(p, n, Color.Red)).Transform(Matrix.CreateScale(5f * scale, 20 * scale, 5f * scale));
            var b = new Cylinder(10, (p, n) => new ColorVertex(p, n, Color.Green)).Transform(Matrix.CreateScale(5f * scale, 20 * scale, 5f * scale) * Matrix.CreateRotationX(MathHelper.PiOver2));
            var c = new Cylinder(10, (p, n) => new ColorVertex(p, n, Color.Blue)).Transform(Matrix.CreateScale(5f * scale, 20 * scale, 5f * scale) * Matrix.CreateRotationZ(MathHelper.PiOver2));

            var d = new Cube((p, n) => new ColorVertex(p, n, Color.Black)).Transform(Matrix.CreateScale(15f * scale));

            var e1 = new Sphere(2, (p, n) => new ColorVertex(p, n, Color.White)).Transform(Matrix.CreateScale(1.5f * scale) * Matrix.CreateTranslation(new Vector3(7.5f, 7.5f, 7.5f) * scale));
            var e2 = new Sphere(2, (p, n) => new ColorVertex(p, n, Color.White)).Transform(Matrix.CreateScale(1.5f * scale) * Matrix.CreateTranslation(new Vector3(-7.5f, 7.5f, 7.5f) * scale));
            var e3 = new Sphere(2, (p, n) => new ColorVertex(p, n, Color.White)).Transform(Matrix.CreateScale(1.5f * scale) * Matrix.CreateTranslation(new Vector3(7.5f, -7.5f, 7.5f) * scale));
            var e4 = new Sphere(2, (p, n) => new ColorVertex(p, n, Color.White)).Transform(Matrix.CreateScale(1.5f * scale) * Matrix.CreateTranslation(new Vector3(-7.5f, -7.5f, 7.5f) * scale));
            var e5 = new Sphere(2, (p, n) => new ColorVertex(p, n, Color.White)).Transform(Matrix.CreateScale(1.5f * scale) * Matrix.CreateTranslation(new Vector3(7.5f, 7.5f, -7.5f) * scale));
            var e6 = new Sphere(2, (p, n) => new ColorVertex(p, n, Color.White)).Transform(Matrix.CreateScale(1.5f * scale) * Matrix.CreateTranslation(new Vector3(-7.5f, 7.5f, -7.5f) * scale));
            var e7 = new Sphere(2, (p, n) => new ColorVertex(p, n, Color.White)).Transform(Matrix.CreateScale(1.5f * scale) * Matrix.CreateTranslation(new Vector3(7.5f, -7.5f, -7.5f) * scale));
            var e8 = new Sphere(2, (p, n) => new ColorVertex(p, n, Color.White)).Transform(Matrix.CreateScale(1.5f * scale) * Matrix.CreateTranslation(new Vector3(-7.5f, -7.5f, -7.5f) * scale));

            var e9 = new Sphere(2, (p, n) => new ColorVertex(p, n, Color.HotPink)).Transform(Matrix.CreateScale(2f * scale));

            var abc = a.Clone();
            abc.Union(b);
            abc.Union(c);

            shapeToRender = d;
            d.Subtract(abc);

            d.Union(e1);
            d.Union(e2);
            d.Union(e3);
            d.Union(e4);
            d.Union(e5);
            d.Union(e6);
            d.Union(e7);
            d.Union(e8);
            d.Union(e9);

            BSP topLeftFront, topLeftBack, topRightBack, topRightFront, bottomLeftFront, bottomLeftBack, bottomRightBack, bottomRightFront;
            d.Split(out topLeftFront, out topLeftBack, out topRightBack, out topRightFront, out bottomLeftFront, out bottomLeftBack, out bottomRightBack, out bottomRightFront);
            shapes = new[] {topLeftFront, topLeftBack, topRightBack, topRightFront, bottomLeftFront, bottomLeftBack, bottomRightBack, bottomRightFront};

            //shapeToRender.Intersect(new Prism(1, new Vector2[]
            //{
            //    new Vector2(-1.2f, -1.2f),
            //    new Vector2(1.2f, -1.2f),
            //    new Vector2(1.2f, 1.2f),
            //    new Vector2(0f, 2f),
            //    new Vector2(-1.2f, 1.2f),
            //}));
            //shapeToRender.Intersect(new Cylinder(1));

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

            MouseState m = Mouse.GetState();
            Vector3 start = GraphicsDevice.Viewport.Unproject(new Vector3(m.X, m.Y, 0), projection, view, Matrix.Identity);
            Vector3 end = GraphicsDevice.Viewport.Unproject(new Vector3(m.X, m.Y, 1), projection, view, Matrix.Identity);
            Ray r = new Ray(start, Vector3.Normalize(end - start));

            float? distance = r.Intersects(shapeToRender);
            if (!distance.HasValue)
                mouseIntersectionPosition = null;
            else
                mouseIntersectionPosition = r.Position + r.Direction * distance.Value;
            Window.Title = mouseIntersectionPosition.HasValue.ToString();

            shapeToRender = shapes[((int)gameTime.TotalGameTime.TotalSeconds) % shapes.Length];

            base.Update(gameTime);
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

            GraphicsDevice.RasterizerState = solidState;
            GraphicsDevice.DepthStencilState = new DepthStencilState()
            {
                DepthBufferEnable = true,
                DepthBufferWriteEnable = true,
            };

            float time = (float)gameTime.TotalGameTime.TotalSeconds / 1.5f;
            DrawShape(effect, shapeToRender, Matrix.CreateRotationY(time) * Matrix.CreateRotationX(time) * Matrix.CreateTranslation(0, -0.5f, 0), Color.Green);

            //if (mouseIntersectionPosition.HasValue)
            //    DrawShape(effect, sphere, Matrix.CreateTranslation(mouseIntersectionPosition.Value), Color.Black);

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
