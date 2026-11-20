Imports Microsoft.AspNetCore.Builder
Imports Microsoft.AspNetCore.Http
Imports System.IO

Public Module Program
        Public Sub Main(args As String())
            Dim repositoryRoot = FindRepositoryRoot(AppContext.BaseDirectory)
            Dim options As New WebApplicationOptions With {
                .Args = args,
                .ContentRootPath = If(repositoryRoot, Directory.GetCurrentDirectory()),
                .WebRootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot")
            }
            Dim builder = WebApplication.CreateBuilder(options)
            Dim app = builder.Build()

            app.UseDefaultFiles()
            app.UseStaticFiles()

            ' The readings are generated artifacts in the repository's dist/data folder.
            ' This route lets the existing date-navigation UI continue to request the
            ' same JSON URLs when hosted by ASP.NET Core.
            app.MapGet("/data/{isoDate}.json", Function(isoDate As String) As IResult
                                                  Dim parsedDate As DateOnly
                                                  If Not DateOnly.TryParseExact(isoDate, "yyyy-MM-dd", Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.None, parsedDate) Then
                                                      Return Results.BadRequest(New With {.error = "Date must use yyyy-MM-dd."})
                                                  End If

                                                  Dim dataRoot = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "dist", "data"))
                                                  Dim filePath = Path.GetFullPath(Path.Combine(dataRoot, isoDate & ".json"))
                                                  If Not filePath.StartsWith(dataRoot & Path.DirectorySeparatorChar, StringComparison.Ordinal) OrElse Not File.Exists(filePath) Then
                                                      Return Results.NotFound()
                                                  End If

                                                  Return Results.File(filePath, "application/json; charset=utf-8")
                                              End Function)

            app.MapGet("/health", Function() Results.Ok(New With {.status = "ok"}))
            app.MapFallbackToFile("index.html")
            app.Run()
        End Sub

        Private Function FindRepositoryRoot(startAt As String) As String
            Dim current As DirectoryInfo = New DirectoryInfo(startAt)
            While current IsNot Nothing
                If File.Exists(Path.Combine(current.FullName, "scripts", "build.mjs")) Then Return current.FullName
                current = current.Parent
            End While
            Return Nothing
        End Function
End Module
