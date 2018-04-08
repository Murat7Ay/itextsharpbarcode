using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BarcodePDF
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            using (Stream inputPdfStream = new FileStream(Server.MapPath("~") + "/pdf/test.pdf", FileMode.Open, FileAccess.Read, FileShare.Read))
            using (MemoryStream outputPdfStream = new MemoryStream())
            {
                var reader = new PdfReader(inputPdfStream);
                var stamper = new PdfStamper(reader, outputPdfStream);
                var pdfContentByte = stamper.GetOverContent(1);
                for (int i = 0; i < 5; i++)
                {
                    iTextSharp.text.Image image = GetBarcodeImage();
                    image.SetAbsolutePosition(50, 382 + i * 70);
                    pdfContentByte.AddImage(image);
                }
                stamper.Close();
                var newDoc = GetDocumentStream();
                var byteList = new List<byte[]>();
                byteList.Add(outputPdfStream.ToArray());
                byteList.Add(newDoc.ToArray());
                var mergedPdf = MergePdfForms(byteList);
                Response.Clear();
                Response.AddHeader("content-disposition", "inline;filename=Telekurye.pdf");
                Response.ContentType = "application/pdf";
                Response.BinaryWrite(mergedPdf.ToArray());
                Response.End();
            }
        }

        private string GetData()
        {
            var chars = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0"};
            var data = chars.OrderBy(x => Guid.NewGuid()).Aggregate((x, y) => x + y).ToString();
            return data;
        }
        private enum BarcodeTag
        {
            TK,
            AY,
            MRT
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            var doc = new Document(PageSize.A4);
            using (System.IO.MemoryStream outputPdfStream = new System.IO.MemoryStream())
            {
                var writer = PdfWriter.GetInstance(doc, outputPdfStream);
                doc.Open();
                int tableColumns = 5;
                PdfPTable aTable = new PdfPTable(tableColumns);
                aTable.SetWidthPercentage(new[] { 100f, 100f, 100f, 100f, 100f }, PageSize.A4);
                for (int i = 0; i <36; i++)
                {
                    aTable.AddCell(GetBarcodeImage());
                    aTable.AddCell(GetBarcodeImage());
                    aTable.AddCell(GetBarcodeImage());
                    aTable.AddCell(GetBarcodeImage());
                    aTable.AddCell(GetBarcodeImage());
                }
                doc.Add(aTable);
                doc.Close();
                Response.Clear();
                Response.AddHeader("content-disposition", "inline;filename=Empty.pdf");
                Response.ContentType = "application/pdf";
                Response.BinaryWrite(outputPdfStream.ToArray());
                Response.End();
            }
        }
        public MemoryStream MergePdfForms(List<byte[]> files)
        {
            if (files.Count > 1)
            {
                PdfReader pdfFile;
                Document doc;
                PdfWriter pCopy;
                MemoryStream msOutput = new MemoryStream();
                pdfFile = new PdfReader(files[0]);
                doc = new Document();
                pCopy = new PdfSmartCopy(doc, msOutput);
                doc.Open();
                for (int k = 0; k < files.Count; k++)
                {
                    pdfFile = new PdfReader(files[k]);
                    for (int i = 1; i < pdfFile.NumberOfPages + 1; i++)
                    {
                        ((PdfSmartCopy)pCopy).AddPage(pCopy.GetImportedPage(pdfFile, i));
                    }
                    pCopy.FreeReader(pdfFile);
                }
                pdfFile.Close();
                pCopy.Close();
                doc.Close();
                return msOutput;
            }
            else if (files.Count == 1)
            {
                return new MemoryStream(files[0]);
            }
            return null;
        }
        private MemoryStream GetDocumentStream()
        {
            var doc = new Document(PageSize.A4);
            using (System.IO.MemoryStream outputPdfStream = new System.IO.MemoryStream())
            {
                var writer = PdfWriter.GetInstance(doc, outputPdfStream);
                doc.Open();
                int tableColumns = 5;
                PdfPTable aTable = new PdfPTable(tableColumns);
                aTable.SetWidthPercentage(new[] { 100f, 100f, 100f, 100f, 100f }, PageSize.A4);
                for (int i = 0; i < 36; i++)
                {
                    aTable.AddCell(GetBarcodeImage());
                    aTable.AddCell(GetBarcodeImage());
                    aTable.AddCell(GetBarcodeImage());
                    aTable.AddCell(GetBarcodeImage());
                    aTable.AddCell(GetBarcodeImage());
                }
                doc.Add(aTable);
                doc.Close();
                return outputPdfStream;
            }
        }
        private iTextSharp.text.Image GetBarcodeImage()
        {
            var barcode = new BarcodeLib.Barcode();
            BaseColor color = null;
            var inputImageStream = barcode.Encode(BarcodeLib.TYPE.CODE39Extended, GetData(), Color.Black, Color.White, 250, 50);
            iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(inputImageStream, color);
            return image;
        }
        
    }
}