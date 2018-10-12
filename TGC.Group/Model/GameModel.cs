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
using System;
using TGC.Group.Camara;

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
        //Boleano para ver si dibujamos el boundingbox
        private bool BoundingBox { get; set; }
        private float movimientoEscenario = 957f;
        private List<TgcScene> escenas = new List<TgcScene>(); //lista de instancis de meshes que son escenarios
        private TgcMesh personaje;
        private float velocidadDesplazamientoPersonaje = 250f;
        private camaraTerceraPersona camaraInterna;
        //private int cantidadEscenas = 10;

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

            //cargo y acomodo personaje
            personaje = loader.loadSceneFromFile(MediaDir + "Bloque1\\personaje-TgcScene.xml").Meshes[0];
            personaje.AutoTransform = true;     
            personaje.Move(65f, 15f, -200f);

            //cargo las escenas
            var escena1 = loader.loadSceneFromFile(MediaDir + "Bloque1\\escenario1-TgcScene.xml");
            var escena2 = loader.loadSceneFromFile(MediaDir + "Bloque1\\escenario2-TgcScene.xml");
            var escena3 = loader.loadSceneFromFile(MediaDir + "Bloque1\\escenario3-TgcScene.xml");

            escenas.Add(escena1);
            escenas.Add(escena2);
            escenas.Add(escena3);
            calcularPrimeraDisposicionAleatoriaDeEscenarios();


            camaraInterna = new camaraTerceraPersona(personaje.Position, 200, -500);
            Camara = camaraInterna;
        }

        public override void Update()
        {
            PreUpdate();

            var movement = TGCVector3.Empty;

            //Capturar Input teclado
            if (Input.keyPressed(Key.F))
            {
                BoundingBox = !BoundingBox;
            }

            if (Input.keyDown(Key.Right))
            {
                movement.X = 1;           
            }
            else if(Input.keyDown(Key.Left))
            {
                movement.X = -1;
            }

            if (Input.keyDown(Key.Up))
            {
                movement.Z = 1;
            }
            else if (Input.keyDown(Key.Down))
            {
                movement.Z = -1;
            }

            movement *= velocidadDesplazamientoPersonaje * ElapsedTime;
            personaje.Move(movement);

            camaraInterna.Target = personaje.Position;
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

            //Render personaje
            personaje.Render();
            //Render escenas
            RenderEscenas(); 
            //Render instancias escenas            

            //Render de BoundingBox, muy útil para debug de colisiones.
            if (BoundingBox)
            {
                BoundingBoxPersonajeYObstaculos();
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
            disposeEscenas();
            personaje.Dispose();
        }

        //FUNCIONES PRIVADAS

        private void disposeEscenas()
        {
            for (int i = 0; i < escenas.Count(); i++)
            {
                escenas[i].DisposeAll();
            }
        }

        private void BoundingBoxPersonajeYObstaculos()
        {
            personaje.BoundingBox.Render();
            /*for (int i = 0; i < obstaculos.Count(); i++)
            {
                obstaculos[i].BoundingBox.Render();
            } */
        }
  
        private void RenderEscenas()
        {
            for (int i = 0; i < escenas.Count(); i++)
            {
                escenas[i].RenderAll();
            }
        }            

        private void calcularPrimeraDisposicionAleatoriaDeEscenarios()
        {
            float largo = 0;    
            //Random fabricaNumerosAleatorios = new Random();
            //HashSet<int> historialNumeros = new HashSet<int>();

            for (int i=0; i < escenas.Count(); i++)
            {                                  
                for (int k = 0; k < escenas[i].Meshes.Count(); k++)
                {
                    escenas[i].Meshes[k].Move(0f,0f,largo);   
                }
                    
                largo += movimientoEscenario;
            }                   
        }



    }
}