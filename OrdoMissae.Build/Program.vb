Imports System.Diagnostics
Imports System.IO

Public Module Program
        Public Function Main(args As String()) As Integer
            Dim root = FindRepositoryRoot(AppContext.BaseDirectory)
            If root Is Nothing Then
                Console.Error.WriteLine("Could not locate scripts/build.mjs. Run this command from the Ordo Missae repository.")
                Return 2
            End If

            Dim startInfo As New ProcessStartInfo With {
                .FileName = "node",
                .WorkingDirectory = root,
                .UseShellExecute = False
            }
            startInfo.ArgumentList.Add(Path.Combine(root, "scripts", "build.mjs"))
            For Each argument In args
                startInfo.ArgumentList.Add(argument)
            Next

            Using childProcess = Process.Start(startInfo)
                If childProcess Is Nothing Then
                    Console.Error.WriteLine("Unable to start the existing data build process.")
                    Return 2
                End If
                childProcess.WaitForExit()
                Return childProcess.ExitCode
            End Using
        End Function

        Private Function FindRepositoryRoot(startAt As String) As String
            Dim current As DirectoryInfo = New DirectoryInfo(startAt)
            While current IsNot Nothing
                If File.Exists(Path.Combine(current.FullName, "scripts", "build.mjs")) Then Return current.FullName
                current = current.Parent
            End While
            Return Nothing
        End Function
End Module
