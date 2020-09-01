# Servicio_eComm
#Senecesita la instalacion de CURL. https://curl.haxx.se/download.html
Este servicio se ejecuta de manera periodica para la actualizaciòn de registros en la base de datos.
El ejecutable puede ir en cualquier ruta dentro de la unidad C.
En el servidor en donde se instale debe de tener acceso a la BD para que pueda actualizar sin problema.
En el arcihvo de configuración se pueden establecer los valores con los cuales se ejecutaran las actualizaciones de manera periodica.

#Estos valores son en milisegundos
#  < add key="intervalES" value="14400000"/ >
# < add key="intervalES_Bck" value="13500000"/ >
# < add key="intervalCorreo" value="300000"/ >
# < add key="intervaloCancelaRef" value="400000"/ >

#En esta sección del archivo de configuracion se establecen las rutas del elastic search en donde se encuentra el archivo que contiene
#la consulta que indexa los datos

#Esta ruta es la del archivo bat de elastic search
# < add key="pathES" value="X:\logstash-7.3.2\bin\logstash.bat"/ >

#Esta ruta es la del archivo conf en donde se coloca la consulta que extrae la informaciòn de los productos.
# < add key="parametersES" value=" -f X:\logstash-7.3.2\config\logstash.conf"/>
# < add key="parametersES_Bck" value=" -f X:\logstash-7.3.2\config\logstash_bck.conf"/>

#Esta ruta es la del programa CURL que se debe de instalar previamente en el servidor
# < add key="pathCurl" value="C:\curl-7.68.0-win64-mingw\bin\curl.exe"/>

#Esta ruta es la de los archivos ya indexados que deben de estar en mantenimiento.
# < add key="parametersCurl" value=" -XDELETE localhost:9200/productospapeleria"/>
# < add key="parametersCurl_Bck" value=" -XDELETE localhost:9200/productospapeleria_bck"/>

#Para el tema de la facturacion el servicio necesita estas rutas en donde se generan los XML y los PDF
# < add key="fileOrigin" value=X:..\TimboxLayout\Timbrados\"/>

#Una vez que se envian por correo se mueven para esta ruta.
# < add key="fileDestination" value="..\Content\files\"/>

#### Instalar el ejecutable como servicio Windows
#https://docs.microsoft.com/en-us/dotnet/framework/windows-services/how-to-install-and-uninstall-services

#El ejectubale que se debe de instalar como servicio es el que se encuentra en la carpeta ServicesEcomm.zip
