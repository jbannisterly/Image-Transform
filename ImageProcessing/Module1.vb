Imports System.Console
Module Module1

    Const sh As Integer = 180
    Const sw As Integer = 240
    Const START_INDEX As Integer = 1000

    Sub Main()
        Dim tt As Double = Timer

        Dim reader As IO.Stream = FFmpegWrapper.ReadImageStream("targets/target.mp4", sw, sh, 30)

        'Dim image As Image
        'Dim c As Integer = 0
        '
        'image = FFmpegWrapper.ReadImage(reader, sw, sh)
        'While Not IsNothing(image)
        '    image = FFmpegWrapper.ReadImage(reader, sw, sh)
        '    c += 1
        'End While
        'WriteLine(c - 1 & " images")
        '
        'Dim targetImages As Image() = FFmpegWrapper.VideoToImages("targets/target.mp4", sw, sh)

        Dim locations As String() = {
            "components/video1.mp4",
            "components/video7.mp4",
            "components/video12.mp4",
            "components/video23.mp4",
            "components/video27.mp4",
            "components/video33.mp4",
            "components/video18.mp4"
            }

        Dim componentImages As Image() = FFmpegWrapper.VideoToImages(locations, sw, sh, 0.1, 30)
        For i = 0 To componentImages.Length - 1
            componentImages(i) = componentImages(i).ShiftConstant(-componentImages(i).GetMeanValue())
        Next
        Dim nImages As Integer = componentImages.Length - 1

        Dim t As Double = Timer()
        Dim relativeSim As Single()

        Dim targetImage As Image
        Dim k As Integer = 1
        WriteLine("loaded")
        Do
            targetImage = FFmpegWrapper.ReadImage(reader, sw, sh)
            If Not IsNothing(targetImage) And k > START_INDEX Then
                Dim composite As New Image(sw, sh, New Single(sw * sh - 1) {})
                Dim similarity As Single = 0
                Dim maxSimilarity As Single = 0
                Dim maxSimIndex As Integer = -1

                composite = composite.ShiftConstant(targetImage.GetMeanValue)
                Dim modifiedtarget As Image = targetImage.ShiftConstant(targetImage.GetMeanValue)

                For i = 1 To nImages Step 1
                    componentImages(i).lastSim = modifiedtarget.GetSimilarity(componentImages(i))
                Next

                For j = 1 To 32
                    maxSimilarity = 0
                    For i = 1 To nImages
                        similarity = componentImages(i).lastSim
                        If similarity > maxSimilarity Or -1 * similarity > maxSimilarity Then
                            maxSimilarity = Math.Abs(similarity)
                            maxSimIndex = i
                        End If
                    Next
                    similarity = componentImages(maxSimIndex).lastSim / componentImages(maxSimIndex).GetMagnitude
                    composite = composite.Add(componentImages(maxSimIndex), similarity)
                    relativeSim = componentImages(maxSimIndex).GetRelativeSim(componentImages)
                    For i = 1 To componentImages.Length - 1
                        componentImages(i).lastSim -= similarity * relativeSim(i)
                    Next
                Next

                composite = composite.Sharpen()

                'IO.File.WriteAllBytes("debug\i" & k & "_output.bmp", composite.ImageData)
                'IO.File.WriteAllBytes("debug\i" & k & "_target.bmp", targetImage.ImageData)
                'targetImage = targetImage.Subtract(composite, 1)
                'WriteLine(targetImage.GetMagnitude)
                IO.File.WriteAllBytes("outputs\out" & k & ".bmp", composite.ImageData)
                'WriteLine(k)
            End If
            k += 1
        Loop While Not IsNothing(targetImage)
        WriteLine(Timer - t)

        FFmpegWrapper.ImagesToVideo()


    End Sub



End Module
