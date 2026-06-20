

1. plugins de XR



2. xR plugin management: 


3. XR : extended reality devices. Ejmplo todos los visores como metaquest


3. cambio de escenas: SceneManager.LoadScene
 Escena Core/Persistent: Tienes una escena principal que nunca se descarga. Aquí adentro vive únicamente el OVRCameraRig (tus ojos y tus manos de Meta) y los scripts globales del juego (audio, puntaje, etc.).

Escenas Aditivas (Visuales): Las demás escenas (Menú Principal, Nivel 1, Interfaz de Visuales) se cargan dentro de la escena principal de forma aditiva (LoadSceneMode.Additive).

4. SDK INteraction: Permite que el objeto sea agarrable . SI lo mnaerjas con controllers , eontces no necesitas que sea agarrable con la mano. Mas simple y limpio genera menos bugs. 
 - Al añadir esta capacidad, se crea en automatico  adjunto un archivo "ISDK..." . Si lo elimino, entonces se elimina la SDK interaction

5. Heredando coordenadas: 
  - Genera problemas si no lo entiendes. Todo hijo toma su posición respecto al padre. Si posición del hijo es 0 ; 0 ; 0, entonces esta acuplado en la misma posición del padre. 
Cuando mueves un objeto dentro de un "Padre" en Unity, este hereda las coordenadas del padre. Si tu sable estaba en la posición (X: 5, Y: 2, Z: -1) en el mundo, ahora estará a 5 metros de distancia de tu mano virtual.

6. IMplemetnacion de cosliioens estilo beat saber
Es complejo por lo que dividiremos en 3 fases. Realizaremos tecnica de reemplazo de objetos
1. fase 1: crear los objetos de reemplao

2. fase 2: Añadir añari gravedad y movimiento a las partes
 - para que no simplemente aprezcan y se caigan. 
 - a cada mitad añadirle rigibody con gravedad y desactiva lo kinematic (no necesitamos tocarlo )
 - El Script de Impulso (SlicedPart.cs): Crearemos un mini-script genérico que se le pega a cada mitad. En su Start(), este script le aplicará una fuerza física hacia afuera (X negativa para la izquierda, X positiva para la derecha) y un torque (rotación aleatoria). Esto hará que salgan volando con un efecto "explosivo" muy satisfactorio al aparecer.

 7. Origen del cubo: Archivo .fbx = es un import 3d Externo, no creado con unity

 8. Pront a futueo luego de probar lo queteinamos
 ""
 okey te comunico que las piezas se separa n individualmente. 

Ahora si requiero que analicemos la situacion actual y determinemos plan A y plan B y la estregia de pasos a seguir. 

AUn no 



Sin embargo querio aclara que preferiria una opcion que nom e tome gran tiempo conseguir
 ""
  