Public Class Bitmap
    Private raw As Byte()
    Private height As Integer
    Private width As Integer
    Private scanline As Integer
    Private grey As Single() = {}

    Sub New(raw As Byte())
        Me.raw = raw
        Me.height = DataUtil.ReadInt(raw, &H16)
        Me.width = DataUtil.ReadInt(raw, &H12)
        scanline = Math.Ceiling(width / 4) * 4
    End Sub

    Public Function GetGrey(height As Integer, width As Integer) As Single()
        Dim index As Integer
        If grey.Length = 0 Then
            grey = New Single(height * width - 1) {}
            For i = 0 To height - 1
                For j = 0 To width - 1
                    index = (Math.Floor(i * Me.height / height) * scanline + Math.Floor(j * Me.width / width)) * 3 + 54
                    grey(i * width + j) = (CSng(raw(index)) + raw(index + 1) + raw(index + 2)) / 255 / 3 * 2 - 1
                Next
            Next
        End If
        Return grey
    End Function
End Class
