# Sifen-XSD-Downloader
 
Aplicación de consola

Esta utilidad realiza la descarga de los esquemas actualizados provistos por la entidad tributaria de Parguay.

Mediante un menú sencillo, se solicita al usuario los siguientes datos:

* URL: Dirección web donde se publican los esquemas
* Carpeta de salida: Ubicación donde serán descargados los esquemas
* Reemplazar archivos: Indicar si desea reemplazar esquemas existentes en la carpeta de salida
* Ubicación del archivo XSD.exe: Se requiere dicho archivo para crear una clase de C# basada en un esquemas, mediante el comando XSD
* Nombre del primer archivo: El comando XSD necesita la ubicación del esquema en que se basará para crear el archivo .cs