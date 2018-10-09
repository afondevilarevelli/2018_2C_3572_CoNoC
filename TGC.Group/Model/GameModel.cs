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
using System.Collections.Generic;

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
        private string Heightmap;
        private string texturaHeightmap;
        TgcSimpleTerrain terreno1 = new TgcSimpleTerrain();
        TgcSimpleTerrain terreno2 = new TgcSimpleTerrain();
        //Boleano para ver si dibujamos el boundingbox
        private bool BoundingBox { get; set; }
        private List<TgcScene> escenas = new List<TgcScene>();
        

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

            escenas.Add(loader.loadSceneFromFile(MediaDir + "Bloque1\\vegetacion1-TgcScene.xml"));
            escenas.Add(loader.loadSceneFromFile(MediaDir + "Bloque1\\vegetacion2-TgcScene.xml"));      

            Camara = new CamaraExploradora(new TGCVector3(2511f, 1125f, 150f), Input);

            
            //Path de Heightmap default del terreno 
            Heightmap = MediaDir + "Bloque1\\" + "heightmapTP.jpg";
            terreno1.loadHeightmap(Heightmap, 20.0f, 1.3f, new TGCVector3(0.0f, 0.0f, 0.0f));
            //Path de Textura default del terreno y Modifier para cambiarla
            texturaHeightmap = MediaDir + "Bloque1\\" + "tgcPisoEscenario.png";
            terreno1.loadTexture(texturaHeightmap);

            //Path de Heightmap default del terreno 
            Heightmap = MediaDir + "Bloque1\\" + "heightmapTP.jpg";
            terreno2.loadHeightmap(Heightmap, 20.0f, 1.3f, terreno1.Center + new TGCVector3(50, 0.0f, 0.0f));
            //Path de Textura default del terreno y Modifier para cambiarla
            texturaHeightmap = MediaDir + "Bloque1\\" + "tgcPisoEscenario.png";
            terreno2.loadTexture(texturaHeightmap);

            moverMeshesEscenas();

            quitarMeshesOriginalesEscenas();
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

            //Render terrain
            terreno1.Render();
            terreno2.Render();
            //Render escena
            RenderEscenas(); //NO SE COMO IR MOVIENDO LAS ESCENAS CON LOS HEIGHTMAPS!!!!!!!!!!!!!!!!!!!!

            //Render de BoundingBox, muy útil para debug de colisiones.
            if (BoundingBox)
            {
                BoundingBoxEscenas();
            }
						
			PostRender();
        }

        /// <summary>
        ///     Se llama cuando termina la ejecución del ejemplo.
        ///     Hacer Dispose() de todos los objetos creados.
        ///     Es muy importante liberar los recursos, sobretodo los gráficos ya que quedan bloqueados en el device de video.
        /// </summary>
        public override void Dispose()
        {
            DisposeEscenas();
            terreno1.Dispose();
            terreno2.Dispose();
        }

        //FUNCIONES PRIVADAS

        private void DisposeEscenas()
        {
            for(int i=0; i < escenas.Count(); i++)
            {
                escenas[i].DisposeAll();
            }
        }

        private void BoundingBoxEscenas()
        {
            for (int i = 0; i < escenas.Count(); i++)
            {
                escenas[i].BoundingBox.Render();
            }
        }

        private void RenderEscenas()
        {
            for (int i = 0; i < escenas.Count(); i++)
            {
                escenas[i].RenderAll();
            }
        }

        //IMPORTANTE PARA RENDERIZAR BIEN LA ESCENA
        private void quitarMeshesOriginalesEscenas()
        {
            for (int i = 0; i < escenas.Count(); i++)
            {
                for (int j = 0; j < escenas[i].Meshes.Count(); j++)
                {
                    if (escenas[i].Meshes[j].MeshInstances.Count() != 0)
                    {
                        escenas[i].Meshes.Remove(escenas[i].Meshes[j]);
                    }
                }
            }
        }       
     
        private void moverMeshesEscenas(){
        
        }


    }
}
