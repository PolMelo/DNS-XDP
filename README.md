# DNS-XDP

Aplicacion de escritorio para Windows que permite cambiar los servidores DNS de
los adaptadores de red activos y restaurar la configuracion anterior.

## Descargar y ejecutar

1. Descarga `DNS-XDP-v1.0.0-windows.zip` desde la seccion **Releases**.
2. Descomprime el archivo en una carpeta.
3. Ejecuta `DnsSwitcher.exe` y acepta el aviso de administrador de Windows.
4. Pulsa **Activar** para aplicar los DNS o **Desactivar** para restaurar los
   anteriores.

La aplicacion requiere Windows y .NET Framework 4.8. No es un instalador; se
ejecuta desde la carpeta descomprimida.

## DNS configurados

- `85.208.114.51`
- `51.170.52.10`

Al activar, la aplicacion guarda la configuracion DNS anterior en
`%ProgramData%\DnsSwitcher\backup.txt`. Al cerrar, intenta restaurarla.

## Compilar desde el codigo fuente

Abre `DnsSwitcher.csproj` con Visual Studio o ejecuta desde una terminal con el
SDK de .NET:

```powershell
dotnet build .\DnsSwitcher.csproj --configuration Release
```

El ejecutable se genera en `bin\Release\net48\DnsSwitcher.exe`.
