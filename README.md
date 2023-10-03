**Comandos para la migracion (hay que cambiar el connection string del app settings):** 
- cd api3 // Caperta del .Csproj
- dotnet tool install --global dotnet-ef // Por si no tiene todos los comandos de entity framework
- dotnet ef database update firstmigration --project api3.csproj // Actualizar la base de datos

**Por caica**
- dotnet ef migrations add firstmigration --project api3.csproj // Hacer la migracion
- dotnet ef migrations remove // Por si por alguna razon la migration falla (No deberia)

# Rest API Avazanda 
Equipo 2
- Sebastian fernandez #1102556 (cvstian)
- Rayfel Ogando #1107535 (Rayfel)
- Avis Zucco #1104970 (arinaploplo)
- Guillermo Jorge #1107266 (guillermusmax)
