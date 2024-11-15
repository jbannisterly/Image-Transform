Public Class Image
    Private width As Integer
    Private height As Integer
    Private data As Single()
    Private sumsquare As Single = -1
    Private magnitude As Single = -1
    Public lastSim As Single = 0
    Private relativeSim As Single()

    Public Sub New(width As Integer, height As Integer, path As String)
        Me.width = width
        Me.height = height
        Me.data = New Bitmap(IO.File.ReadAllBytes(path)).GetGrey(height, width)
    End Sub

    Public Sub New(width As Integer, height As Integer, data As Single())
        Me.width = width
        Me.height = height
        Me.data = data
    End Sub

    Public Function GetSimilarity(compare As Image) As Single
        Dim compareData As Single() = compare.data
        Dim similarity As Single = 0
        For i = 0 To data.Length - 1
            similarity += compareData(i) * data(i)
        Next
        'Return similarity / compare.GetSumSquare()
        Return similarity / compare.GetMagnitude()
    End Function

    Public Function GetSumSquare() As Single
        Dim sum As Single = 0
        If sumsquare = -1 Then
            For i = 0 To data.Length - 1
                sum += data(i) * data(i)
            Next
            sumsquare = sum
        End If
        Return sumsquare
    End Function

    Public Function GetMagnitude() As Single
        If magnitude = -1 Then
            magnitude = Math.Sqrt(GetSumSquare())
        End If
        Return magnitude
    End Function

    Public Function Subtract(toSubtract As Image, multiplier As Single) As Image
        Dim newData(data.Length - 1) As Single
        For i = 0 To data.Length - 1
            newData(i) = data(i) - toSubtract.data(i) * multiplier
        Next
        Return New Image(Me.width, Me.height, newData)
    End Function

    Public Function Add(toAdd As Image, multiplier As Single) As Image
        Dim newData(data.Length - 1) As Single
        For i = 0 To data.Length - 1
            newData(i) = data(i) + toAdd.data(i) * multiplier
        Next
        Return New Image(Me.width, Me.height, newData)
    End Function

    Public Function ImageData() As Byte()
        Dim data As Byte() = IO.File.ReadAllBytes("template.bmp")
        Dim value As Integer
        For i = 0 To height - 1
            For j = 0 To width - 1
                For k = 0 To 2
                    value = (Me.data(i * width + j) + 1) / 2 * 255
                    If value > 255 Then value = 255
                    If value < 0 Then value = 0
                    data((i * width + j) * 3 + k + 54) = value
                Next
            Next
        Next
        Return data
    End Function

    Public Function GetRelativeSim(toCompare As Image()) As Single()
        If IsNothing(relativeSim) Then
            relativeSim = New Single(toCompare.Length - 1) {}
            For i = 1 To toCompare.Length - 1
                relativeSim(i) = GetSimilarity(toCompare(i))
            Next
        End If
        Return relativeSim
    End Function

    Public Function GetMeanValue() As Single
        Dim sum As Single = 0
        For i = 0 To data.Length - 1
            sum += data(i)
        Next
        Return sum / data.Length
    End Function

    Public Function ShiftConstant(constant As Single) As Image
        Dim newData(data.Length - 1) As Single
        For i = 0 To data.Length - 1
            newData(i) = constant + data(i)
        Next
        Return New Image(Me.width, Me.height, newData)
    End Function

    Public Function Sharpen() As Image
        Dim newData(data.Length - 1) As Single
        For i = 0 To data.Length - 1
            newData(i) = Math.Sqrt(Math.Abs(data(i))) * Math.Sign(data(i))
        Next
        Return New Image(Me.width, Me.height, newData)
    End Function
End Class
