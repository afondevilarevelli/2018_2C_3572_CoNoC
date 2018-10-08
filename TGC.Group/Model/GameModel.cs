using Microsoft.DirectX.DirectInput;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.Terrain;
using TGC.Group.Model.Camara;
using System.Linq;
using TGC.Core.Textures;
using TGC.Core.SceneLoader;

namespace TGC.Group.Model
{
    /// <summary>
    ///     Ejemplo para implementar el TP.
    ///     Inicialmente puede ser renombrado o copiado para hacer más ejemplos chicos, en el caso de copiar para que se
    ///     ejecute el nuevo ejemplo deben cambiar el modelo que instancia GameForm <see cref="Form.GameForm.InitGraphics()" />
    ///     line 97.
    /// </summary>
    public class GameModel : TgcExample
    {
        private TgcScene currentScene;
        private string currentHeightmap;
		private string currentTexture;
        TgcSimpleTerrain terreno = new TgcSimpleTerrain();
        //Boleano para ver si dibujamos el boundingbox
        private bool BoundingBox { get; set; }
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

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aquí todo el código de inicialización: cargar modelos, texturas, estructuras de optimización, todo
        ///     procesamiento que podemos pre calcular para nuestro juego.
        ///     Borrar el codigo ejemplo no utilizado.
        /// </summary>

        public override void Init()
        { 
            var loader = new TgcSceneLoader();
            
            currentScene = loader.loadSceneFromFile(MediaDir + "Bloque1\\bloque1a-TgcScene.xml");
            for(int i=0; i<currentScene.Meshes.Count(); i++)
            {
                if (currentScene.Meshes[i].MeshInstances.Count() != 0)
                {
                    currentScene.Meshes.Remove(currentScene.Meshes[i]);
                }
            }

            Camara = new CamaraExploradora(new TGCVector3(2511f, 1125f, 150f), Input);

            
            //Path de Heightmap default del terreno 
            currentHeightmap = MediaDir + "Bloque1\\" + "bloque1a.jpg";
            terreno.loadHeightmap(currentHeightmap, 20.0f, 1.3f, new TGCVector3(0.0f, 0.0f, 0.0f));
            //Path de Textura default del terreno y Modifier para cambiarla
            currentTexture = MediaDir + "Bloque1\\" + "tcgpisoescenario.png";
            terreno.loadTexture(currentTexture);  
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

            //Render terrain
            terreno.Render();


            currentScene.RenderAll();

            //Render de BoundingBox, muy útil para debug de colisiones.
            if (BoundingBox)
            {
                currentScene.BoundingBox.Render();
            }

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
            currentScene.DisposeAll();
            terreno.Dispose();
        }
    }
}
