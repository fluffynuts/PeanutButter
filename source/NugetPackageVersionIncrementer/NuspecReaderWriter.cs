using System;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace NugetPackageVersionIncrementer
{
    public class NuspecReaderWriter
    {
        private readonly string _path;

        public string NuspecXml
        {
            get => _nuspecXml ?? (_nuspecXml = _document?.ToString());
            set
            {
                _nuspecXml = string.IsNullOrWhiteSpace(value)
                    ? "<package></package>" 
                    : value;
                _document = XDocument.Parse(_nuspecXml);
                Subscribe();
            }
        }

        private string _nuspecXml;

        public XDocument Document
        {
            get => _document ?? (_document = XDocument.Parse(_nuspecXml ?? "<package></package>"));
            set
            {
                Unsubscribe();
                _document = value;
                NuspecXml = _document.ToString();
            }
        }

        private XDocument _document;

        public NuspecReaderWriter(string path)
        {
            if (!File.Exists(path))
            {
                throw new ArgumentException(path + " not found", nameof(path));
            }

            NuspecXml = Encoding.UTF8.GetString(File.ReadAllBytes(path));
            _path = path;
        }

        public void Rewrite()
        {
            File.WriteAllBytes(
                _path,
                Encoding.UTF8.GetBytes(NuspecXml)
            );
        }

        private void Subscribe()
        {
            if (_document == null)
            {
                return;
            }
            _document.Changed += ClearNuspecText;
        }

        private void ClearNuspecText(
            object sender, 
            XObjectChangeEventArgs e)
        {
            _nuspecXml = null;
        }

        private void Unsubscribe()
        {
            if (_document == null)
            {
                return;
            }
            _document.Changed -= ClearNuspecText;
        }
    }
}