using Microsoft.DirectX.DirectInput;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Terrain;
using TGC.Group.Model.Camara;

namespace TGC.Group.Model
{
    /// <summary>
    ///     Ejemplo para implementar el TP.
    ///     Inicialmente puede ser renombrado o copiado para hacer más ejemplos chicos, en el caso de copiar para que se
    ///     ejecute el nuevo ejemplo deben cambiar el modelo que instancia GameForm <see cref="Form.GameForm.InitGraphics()" />
    ///     line 97.
    /// </summary>
    public class GameModel : TgcExample
    {/*
		private TGCTextureModifier heightmapModifier;
		private TGCFloatModifier scaleXZModifier;
		private TGCFloatModifier scaleYModifier;
		private TGCTextureModifier textureModifier;*/

		private string currentHeightmap;
		private string currentTexture;
		private Texture terrainTexture;
		private int totalVertices = 300;
		private VertexBuffer vbTerrain;
		/// <summary>
		///     Constructor del juego.
		/// </summary>
		/// <param name="mediaDir">Ruta donde esta la carpeta con los assets</param>
		/// <param name="shadersDir">Ruta donde esta la carpeta con los shaders</param>
		public GameModel(string mediaDir, string shadersDir) : base(mediaDir, shadersDir)
        {
            Category = Game.Default.Category;
            Name = Game.Default.Name;
            Description = Game.Default.Description;
        }

        private TgcScene escena { get; set; }

        //Boleano para ver si dibujamos el boundingbox
        private bool BoundingBox { get; set; }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aquí todo el código de inicialización: cargar modelos, texturas, estructuras de optimización, todo
        ///     procesamiento que podemos pre calcular para nuestro juego.
        ///     Borrar el codigo ejemplo no utilizado.
        /// </summary>

        TgcSimpleTerrain terreno = new TgcSimpleTerrain();
        public override void Init()
        {
            var loader = new TgcSceneLoader(); 
            
            escena = loader.loadSceneFromFile(MediaDir + /* path de la escena*/ /*EJ: */"Bloque1\\bloque1a-TgcScene.xml");
            int i;
           /* TGCVector3 desplazamiento = new TGCVector3();
            for (i = 0; i < escena.Meshes.Count; i++)
            {
                escena.Meshes[0].Position += desplazamiento;
            }*/
            Camara = new CamaraExploradora(new TGCVector3(4700f, 1200f, 1400f), Input);

            
            //Path de Heightmap default del terreno y Modifier para cambiarla
            currentHeightmap = MediaDir + "Bloque1\\" + "bloque1a.jpg";

            terreno.loadHeightmap(currentHeightmap, 50.0f, 1.0f, new TGCVector3(0.0f, 0.0f, 0.0f));
            //createHeightMapMesh(D3DDevice.Instance.Device, currentHeightmap, 25.0f, 1.5f);

            //Path de Textura default del terreno y Modifier para cambiarla
            currentTexture = MediaDir + "Bloque1\\" + "tcgpisoescenario.png";
            terreno.loadTexture(currentTexture);
            //loadTerrainTexture(D3DDevice.Instance.Device, currentTexture);

		}

		public override void Update()
        {
            PreUpdate();

			//TGCVector3 normalLA = TGCVector3.Normalize(Camara.LookAt);

			//Capturar Input teclado
			if (Input.keyPressed(Key.F))
            {
                BoundingBox = !BoundingBox;
            }

			PostUpdate();
        }

		/// <summary>
		///     Crea y carga el VertexBuffer en base a una textura de Heightmap
		/// </summary>
		private void createHeightMapMesh(Microsoft.DirectX.Direct3D.Device d3dDevice, string path, float scaleXZ, float scaleY)
		{
			//parsear bitmap y cargar matriz de alturas
			var heightmap = loadHeightMap(path);

			//Crear vertexBuffer
			totalVertices = 2 * 3 * (heightmap.GetLength(0) - 1) * (heightmap.GetLength(1) - 1);
			vbTerrain = new VertexBuffer(typeof(CustomVertex.PositionTextured), totalVertices, d3dDevice, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionTextured.Format, Pool.Default);

			//Crear array temporal de vertices
			var dataIdx = 0;
			var data = new CustomVertex.PositionTextured[totalVertices];

			//Iterar sobre toda la matriz del Heightmap y crear los triangulos necesarios para el terreno
			for (var i = 0; i < heightmap.GetLength(0) - 1; i++)
			{
				for (var j = 0; j < heightmap.GetLength(1) - 1; j++)
				{
					//Crear los cuatro vertices que conforman este cuadrante, aplicando la escala correspondiente
					var v1 = new TGCVector3(i * scaleXZ, heightmap[i, j] * scaleY, j * scaleXZ);
					var v2 = new TGCVector3(i * scaleXZ, heightmap[i, j + 1] * scaleY, (j + 1) * scaleXZ);
					var v3 = new TGCVector3((i + 1) * scaleXZ, heightmap[i + 1, j] * scaleY, j * scaleXZ);
					var v4 = new TGCVector3((i + 1) * scaleXZ, heightmap[i + 1, j + 1] * scaleY, (j + 1) * scaleXZ);

					//Crear las coordenadas de textura para los cuatro vertices del cuadrante
					var t1 = new TGCVector2(i / (float)heightmap.GetLength(0), j / (float)heightmap.GetLength(1));
					var t2 = new TGCVector2(i / (float)heightmap.GetLength(0), (j + 1) / (float)heightmap.GetLength(1));
					var t3 = new TGCVector2((i + 1) / (float)heightmap.GetLength(0), j / (float)heightmap.GetLength(1));
					var t4 = new TGCVector2((i + 1) / (float)heightmap.GetLength(0), (j + 1) / (float)heightmap.GetLength(1));

					//Cargar triangulo 1
					data[dataIdx] = new CustomVertex.PositionTextured(v1, t1.X, t1.Y);
					data[dataIdx + 1] = new CustomVertex.PositionTextured(v2, t2.X, t2.Y);
					data[dataIdx + 2] = new CustomVertex.PositionTextured(v4, t4.X, t4.Y);

					//Cargar triangulo 2
					data[dataIdx + 3] = new CustomVertex.PositionTextured(v1, t1.X, t1.Y);
					data[dataIdx + 4] = new CustomVertex.PositionTextured(v4, t4.X, t4.Y);
					data[dataIdx + 5] = new CustomVertex.PositionTextured(v3, t3.X, t3.Y);

					dataIdx += 6;
				}
			}

			//Llenar todo el VertexBuffer con el array temporal
			vbTerrain.SetData(data, 0, LockFlags.None);
		}

		/// <summary>
		///     Cargar textura
		/// </summary>
		private void loadTerrainTexture(Microsoft.DirectX.Direct3D.Device d3dDevice, string path)
		{
			//Rotar e invertir textura
			var b = (Bitmap)Image.FromFile(path);
			b.RotateFlip(RotateFlipType.Rotate90FlipX);
			terrainTexture = Texture.FromBitmap(d3dDevice, b, Usage.None, Pool.Managed);
		}

		/// <summary>
		///     Cargar Bitmap y obtener el valor en escala de gris de Y
		///     para cada coordenada (x,z)
		/// </summary>
		private int[,] loadHeightMap(string path)
		{
			//Cargar bitmap desde el FileSystem
			var bitmap = (Bitmap)Image.FromFile(path);
			var width = bitmap.Size.Width;
			var height = bitmap.Size.Height;
			var heightmap = new int[width, height];

			for (var i = 0; i < width; i++)
			{
				for (var j = 0; j < height; j++)
				{
					//Obtener color
					//(j, i) invertido para primero barrer filas y despues columnas
					var pixel = bitmap.GetPixel(j, i);

					//Calcular intensidad en escala de grises
					var intensity = pixel.R * 0.299f + pixel.G * 0.587f + pixel.B * 0.114f;
					heightmap[i, j] = (int)intensity;
				}
			}

			return heightmap;
		}

		/// <summary>
		///     Se llama cada vez que hay que refrescar la pantalla.
		///     Escribir aquí todo el código referido al renderizado.
		///     Borrar todo lo que no haga falta.
		/// </summary>
		public override void Render()
        {
            //Inicio el render de la escena, para ejemplos simples. Cuando tenemos postprocesado o shaders es mejor realizar las operaciones según nuestra conveniencia.
            PreRender();

            //Dibuja un texto por pantalla
            DrawText.drawText("Con la tecla F se dibuja el bounding box.", 0, 20, Color.OrangeRed);
            DrawText.drawText("Con clic izquierdo subimos la camara [Actual]: " + TGCVector3.PrintVector3(Camara.Position), 0, 30, Color.OrangeRed);

            escena.RenderAll();

            //Render de BoundingBox, muy útil para debug de colisiones.
            if (BoundingBox)
            {
                escena.BoundingBox.Render();
            }

            //Render terrain
            terreno.Render();
            /*
			D3DDevice.Instance.Device.SetTexture(0, terrainTexture);
			D3DDevice.Instance.Device.VertexFormat = CustomVertex.PositionTextured.Format;
			D3DDevice.Instance.Device.SetStreamSource(0, vbTerrain, 0);
			D3DDevice.Instance.Device.DrawPrimitives(PrimitiveType.TriangleList, 0, totalVertices / 3);*/
			
			//Finaliza el render y presenta en pantalla, al igual que el preRender se debe para casos puntuales es mejor utilizar a mano las operaciones de EndScene y PresentScene
			PostRender();
        }

        /// <summary>
        ///     Se llama cuando termina la ejecución del ejemplo.
        ///     Hacer Dispose() de todos los objetos creados.
        ///     Es muy importante liberar los recursos, sobretodo los gráficos ya que quedan bloqueados en el device de video.
        /// </summary>
        public override void Dispose()
        {
            escena.DisposeAll();
        }
    }
}
