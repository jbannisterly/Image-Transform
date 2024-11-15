Public Class FFmpegWrapper
    Public Shared Sub ImagesToVideo()
        IO.File.Delete("outputs\out.mp4")
        IO.File.Delete("output.mp4")
        IO.File.Delete("overlay.mp4")

        Dim seq As New ProcessStartInfo()
        seq.CreateNoWindow = False
        seq.UseShellExecute = False
        seq.WorkingDirectory = IO.Directory.GetCurrentDirectory + "\outputs"
        seq.FileName = "ffmpeg"
        seq.Arguments = "-framerate 30 -start_number 1 -i out%d.bmp out.mp4"
        Process.Start(seq).WaitForExit()

        Dim audio As New ProcessStartInfo()
        audio.CreateNoWindow = False
        audio.UseShellExecute = False
        audio.WorkingDirectory = IO.Directory.GetCurrentDirectory
        audio.FileName = "ffmpeg"
        audio.Arguments = "-i outputs/out.mp4 -i targets/target.mp4 -c copy -map 0:v -map 1:a output.mp4"
        Process.Start(audio).WaitForExit()

        Dim overlay As New ProcessStartInfo()
        overlay.CreateNoWindow = False
        overlay.UseShellExecute = False
        overlay.WorkingDirectory = IO.Directory.GetCurrentDirectory
        overlay.FileName = "ffmpeg"
        overlay.Arguments = "-i outputs/out.mp4 -i targets\target.mp4 -filter_complex ""[1]scale=iw/8:ih/8 [pip]; [0][pip] overlay=main_w-overlay_w-1:main_h-overlay_h - 1"" -map 1:a overlay.mp4"
        Process.Start(overlay).WaitForExit()
    End Sub

    Public Shared Function ReadImage(imageStream As IO.Stream, width As Integer, height As Integer) As Image
        Dim buffer(5) As Byte
        Dim imageSize As Integer
        Dim numread As Integer = 1
        Dim imageData As Byte()
        Dim images As New List(Of Image)
        Dim toread As Integer
        Dim totread As Integer

        numread = imageStream.Read(buffer, 0, 6)
        imageSize = DataUtil.ReadInt(buffer, 2)
        imageData = New Byte(imageSize - 1) {}

        totread = numread
        toread = imageSize - numread

        If numread = 0 Then Return Nothing

        While toread > 0 And numread > 0
            numread = imageStream.Read(imageData, totread, toread)
            totread += numread
            toread -= numread
        End While
        Return New Image(width, height, New Bitmap(imageData).GetGrey(height, width))
    End Function

    Public Shared Function ReadImageStream(vidloc As String, width As Integer, height As Integer, framerate As Single) As IO.Stream
        Dim vidtobmp As New ProcessStartInfo()
        vidtobmp.FileName = "ffmpeg"
        vidtobmp.Arguments = "-i " & vidloc & " -c:v bmp -s " & width & "x" & height & " -an -sn -f rawvideo -pixel_format rgb24 -r " & framerate & " pipe:1"
        vidtobmp.UseShellExecute = False
        vidtobmp.RedirectStandardOutput = True
        Dim proc As Process = Process.Start(vidtobmp)
        Return proc.StandardOutput.BaseStream
    End Function

    Public Shared Sub VideoToImages(vidLoc As String, width As Integer, height As Integer, ByRef images As List(Of Image), p As Single, framerate As Integer)
        Dim stream As IO.Stream = ReadImageStream(vidLoc, width, height, framerate)
        Dim buffer(5) As Byte
        Dim imageSize As Integer
        Dim numread As Integer = 1
        Dim imageData As Byte()
        Dim toread As Integer
        Dim totread As Integer
        Dim nextImage As Image
        Dim prevImage As New Image(width, height, New Single(width * height - 1) {})
        Dim prevSimilarity As Single

        numread = stream.Read(buffer, 0, 6)
        imageSize = DataUtil.ReadInt(buffer, 2)
        imageData = New Byte(imageSize - 1) {}

        totread = numread
        toread = imageSize - numread

        While numread > 0
            While toread > 0 And numread > 0
                numread = stream.Read(imageData, totread, toread)
                totread += numread
                toread -= numread
            End While
            nextImage = New Image(width, height, New Bitmap(imageData).GetGrey(height, width))
            prevSimilarity = prevImage.GetSimilarity(nextImage) / prevImage.GetMagnitude()
            If Single.IsNaN(prevSimilarity) Then prevSimilarity = 0
            If prevSimilarity < 0.99 Then
                'Console.WriteLine(images.Count & " " & prevSimilarity)
                'IO.File.WriteAllBytes("debug2\" & images.Count & ".bmp", nextImage.ImageData)
                prevImage = nextImage
                images.Add(nextImage)
            End If
            totread = 0
            toread = imageSize
        End While

    End Sub

    Public Shared Function VideoToImages(locations As String(), width As Integer, height As Integer, p As Single, framerate As Integer) As Image()
        Dim imageComponents As New List(Of Image)
        For i = 0 To locations.Length - 1
            VideoToImages(locations(i), width, height, imageComponents, p, framerate)
        Next
        Return imageComponents.ToArray()
    End Function
End Class
