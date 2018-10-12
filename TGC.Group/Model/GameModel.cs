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
using TGC.Core.Collision;
using static TGC.Core.Collision.TgcCollisionUtils;

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
        private List<TgcScene> escenas = new List<TgcScene>(); 
        private List<TgcMesh> obstaculos = new List<TgcMesh>(); 
        private TgcMesh personaje;
        private float velocidadDesplazamientoPersonaje = 250f;
        private camaraTerceraPersona camaraInterna;
        private readonly float minimoXRuta = -143.6097f;
        private readonly float maximoXRuta = 278.5438f;
        private readonly float minimoZObstaculo = 600f;
        private float anchoRuta;
        private float largoRuta;
        private List<float> posiblesPosicionesObstaculosEnX = new List<float>();
        private bool huboColision = false;

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
            anchoRuta = maximoXRuta - minimoXRuta;

            var loader = new TgcSceneLoader();

            //cargo y acomodo personaje
            personaje = loader.loadSceneFromFile(MediaDir + "Bloque1\\personaje-TgcScene.xml").Meshes[0];
            personaje.AutoTransform = true;
            personaje.Move(65f, 15f, -100f);     

            //cargo las escenas
            var escena1 = loader.loadSceneFromFile(MediaDir + "Bloque1\\escenario1-TgcScene.xml");
            var escena2 = loader.loadSceneFromFile(MediaDir + "Bloque1\\escenario2-TgcScene.xml");
            var escena3 = loader.loadSceneFromFile(MediaDir + "Bloque1\\escenario3-TgcScene.xml");
            var escena4 = loader.loadSceneFromFile(MediaDir + "Bloque1\\escenario4-TgcScene.xml");
            var escena5 = loader.loadSceneFromFile(MediaDir + "Bloque1\\escenario5-TgcScene.xml");

            escenas.Add(escena1);
            escenas.Add(escena2);
            escenas.Add(escena3);
            escenas.Add(escena4);
            escenas.Add(escena5);
            calcularPrimeraDisposicionAleatoriaDeEscenarios();

            //cargo los obstaculos
            var obstaculo1 = loader.loadSceneFromFile(MediaDir + "Bloque1\\obstaculos\\sarcofago-TgcScene.xml").Meshes[0];
            var obstaculo2 = loader.loadSceneFromFile(MediaDir + "Bloque1\\obstaculos\\patrullero-TgcScene.xml").Meshes[0];
            var obstaculo3 = loader.loadSceneFromFile(MediaDir + "Bloque1\\obstaculos\\estatua-TgcScene.xml").Meshes[0];
            var obstaculo4 = loader.loadSceneFromFile(MediaDir + "Bloque1\\obstaculos\\autoDeslizador-TgcScene.xml").Meshes[0];
            var obstaculo5 = loader.loadSceneFromFile(MediaDir + "Bloque1\\obstaculos\\pared-TgcScene.xml").Meshes[0];

            obstaculos.Add(obstaculo1);
            obstaculos.Add(obstaculo2);
            obstaculos.Add(obstaculo3);
            obstaculos.Add(obstaculo4);
            obstaculos.Add(obstaculo5);

            posiblesPosicionesObstaculosEnX.Add(anchoRuta  / 3 + minimoXRuta ); //bien
            posiblesPosicionesObstaculosEnX.Add(anchoRuta / 3 + minimoXRuta + anchoRuta / 6 + anchoRuta/5); //bien
            posiblesPosicionesObstaculosEnX.Add(anchoRuta / 4 + maximoXRuta - anchoRuta / 6); //bien

            calcularPosicionObstaculos();

            camaraInterna = new camaraTerceraPersona(personaje.Position, 130, -500);
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

            if (Input.keyDown(Key.D))
            {
                movement.X = 1;           
            }
            else if(Input.keyDown(Key.A))
            {
                movement.X = -1;
            }

            if (Input.keyDown(Key.W))
            {
                movement.Z = 1;
            }
            else if (Input.keyDown(Key.S))
            {
                movement.Z = -1;
            }

            //Guardar posicion original en X antes de cambiarla
            var originalPosX = personaje.Position.X;

            movement *= velocidadDesplazamientoPersonaje * ElapsedTime;
            personaje.Move(movement);

            huboColision = false;

            //chequeo si hay colision entre bounding boxes
            for (int i = 0; i < obstaculos.Count(); i++)
            {
                var resultadoColision = TgcCollisionUtils.classifyBoxBox(personaje.BoundingBox, obstaculos[i].BoundingBox);
                if (resultadoColision != TgcCollisionUtils.BoxBoxResult.Afuera)
                {      
                    huboColision = true;
                    break;              
                }            
            }

            //chequeo si hubo colision y si supera el ancho de la ruta
            if (huboColision || (personaje.Position.X < minimoXRuta || personaje.Position.X > maximoXRuta) )
            {
                personaje.Position = new TGCVector3(originalPosX, personaje.Position.Y, personaje.Position.Z);
            }

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

            //BackgroundColor
            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            ClearTextures();

            //Dibuja un texto por pantalla
            DrawText.drawText("Con la tecla F se dibuja el bounding box.", 0, 20, Color.OrangeRed);
            DrawText.drawText("Posición del personaje: " + TGCVector3.PrintVector3(personaje.Position), 0, 40, System.Drawing.Color.Red);           
            //Render personaje
            personaje.Render();
            //Render escenas
            RenderEscenas();
            //Render obstaculos 
            RenderObstaculos();
            if (huboColision)
            {
                DrawText.drawText("PERDISTE!!!: ", 500, 20, System.Drawing.Color.DarkViolet);
            }

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
            for (int i = 0; i < obstaculos.Count(); i++)
            {
                obstaculos[i].BoundingBox.Render();
            } 
        }
  
        private void RenderEscenas()
        {
            for (int i = 0; i < escenas.Count(); i++)
            {
                escenas[i].RenderAll();
            }
        }

        private void RenderObstaculos()
        {
            for (int i = 0; i < obstaculos.Count(); i++)
            {
                obstaculos[i].Render();
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
                largoRuta = largo;
            }                   
        }

        private void calcularPosicionObstaculos()
        {
            var espacio = minimoZObstaculo;
            Random aleatorio = new Random();
            var espacioEntreObstaculosMinimo = 700;
            var espacioEntreObstaculosMaximo = 900;
            for (int i = 0; i < obstaculos.Count(); i++)
            {
                obstaculos[i].Position = new TGCVector3(obtenerPosicionObstaculoEnX(), 0f, espacio);
                espacio += aleatorio.Next(espacioEntreObstaculosMinimo, espacioEntreObstaculosMaximo);
            }
        }

        private float obtenerPosicionObstaculoEnX()
        {
            Random aleatorio = new Random();
            int index = aleatorio.Next(0, posiblesPosicionesObstaculosEnX.Count());
            return posiblesPosicionesObstaculosEnX[index];
        }

    }
}