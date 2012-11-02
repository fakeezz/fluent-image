﻿namespace ImageNet.Core
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;

    using ImageNet.Core.Drawing;
    using ImageNet.Core.Filtering;
    using ImageNet.Core.Operations;
    using ImageNet.Core.Resizer;

    public class FluentImage
    {
        private readonly RawImage current;
        
        private FluentImage(Image image)
        {
            this.Image = image;
            this.current = new RawImage(this);
            this.Encoders = new List<EncoderParameter>();
            this.Filtering = new FilteringImpl(this);
        }

        public static FluentImage Create(Image image)
        {
            return new FluentImage(image);
        }

        public static FluentImage Create(string filename)
        {
            var image = Image.FromFile(filename);
            return new FluentImage(image);
        }

        public IList<EncoderParameter> Encoders { get; private set; }

        public IRawImage Current
        {
            get { return this.current; }
        }

        public IResize Resize
        {
            get { return new ResizeImpl(this); }
        }

        public IRotate Rotate
        {
            get { return new RotateImpl(this); }
        }

        public IFlip Turn
        {
            get { return new FlipImpl(this); }
        }

        public IDraw Draw
        {
            get { return new DrawImpl(this); }
        }

        public IFiltering Filters
        {
            get { return this.Filtering; }
        }

        internal Image Image { get; set; }
        internal FilteringImpl Filtering { get; set; }

        public static implicit operator Image(FluentImage builder)
        {
            return builder.Image;
        }

        public void Save(string path)
        {
            this.Save(path, OutputFormat.Jpeg);
        }

        public void Save(string path, OutputFormat format)
        {
            foreach (var filter in this.Filtering.Filters)
            {
                filter.ProcessFilter(this.Image);
            }

            var realPath = string.Format(@"{0}\{1}{2}", Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path), Path.GetExtension(path));
            this.Image.Save(
                realPath,
                ImageCodec.FromOutputFormat(format),
                this.Encoders.Count > 0 ? new EncoderParameters { Param = this.Encoders.ToArray() } : null);
        }

        public override string ToString()
        {
            var imageConverter = new ImageConverter();
            var imageBytes = (byte[])imageConverter.ConvertTo(this.Image, typeof(byte[]));
            return imageBytes != null ? Convert.ToBase64String(imageBytes) : string.Empty;
        }
    }
}