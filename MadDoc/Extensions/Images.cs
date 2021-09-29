using System;
using System.Drawing;
using System.Net.Http;
using System.Drawing.Text;
using System.Threading.Tasks;
using System.Drawing.Drawing2D;
using DSharpPlus;
using DSharpPlus.Entities;

namespace MadDoc.Extensions
{
    public static class Images
    {
        static readonly string[] urls = new string[]
        {
            "https://vgtimes.ru/uploads/games_previews/62203/darkest-dungeon-the-butchers-circus_vgdb.jpg",
            "https://i.pinimg.com/originals/1a/2a/98/1a2a985e572987793728dd3655ace2fc.jpg",
            "https://us.v-cdn.net/5021068/uploads/editor/ou/am9kr0jmftx6.jpg",
            "http://img0.safereactor.cc/pics/post/full/Crusader-%28DD%29-Darkest-Dungeon-%D0%98%D0%B3%D1%80%D1%8B-Louis-Varon-5511776.jpeg", 
            "https://pbs.twimg.com/media/Cj0SswGUkAA6T2Y.jpg"
        };

        static int id = 0;

        public static async Task<string> CreateImageAsync(DiscordUser user)
        {
            Image background;
            if (user.Id != 333193140091486208)
            {
                background = await FetchImageAsync(urls[id++]);
            }
            else
            {
                background = await FetchImageAsync();
            }

            var avatar = await FetchImageAsync(user.GetAvatarUrl(ImageFormat.Png, 2048));

            if (id == 5) id = 0;

            background = CropToBanner(background);
            avatar = ClipImageToCircle(avatar);

            var bitmap = avatar as Bitmap;
            bitmap?.MakeTransparent();

            var banner = CopyRegionIntoImage(bitmap, background);

            if (user.Id == 333193140091486208)
            {
                banner = DrawTextToImage(banner, $"{user.Username}#{user.Discriminator}, создатель...", $"Номер карточки: {user.Id}");
            }
            else
            {
                banner = DrawTextToImage(banner, $"{user.Username}#{user.Discriminator}", $"Номер карточки: {user.Id}");
            }

            string path = $"{Guid.NewGuid()}.png";
            banner.Save(path);
            return await Task.FromResult(path);
        }
         
        private static Bitmap CropToBanner(Image image)
        {
            var originalWidth = image.Width;
            var originalHeight = image.Height;
            var destinationSize = new Size(1100, 450);

            var heightRatio = (float)originalHeight / destinationSize.Height;
            var widthRatio = (float)originalWidth / destinationSize.Width;

            var ratio = Math.Min(heightRatio, widthRatio);

            var heightScale = Convert.ToInt32(destinationSize.Height * ratio);
            var widthScale = Convert.ToInt32(destinationSize.Width * ratio);

            var startX = (originalWidth - widthScale) / 2;
            var startY = (originalHeight - heightScale) / 2;

            var sourceRectangle = new Rectangle(startX, startY, widthScale, heightScale);
            var bitmap = new Bitmap(destinationSize.Width, destinationSize.Height);
            var destinationRectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

            using var g = Graphics.FromImage(bitmap);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(image, destinationRectangle, sourceRectangle, GraphicsUnit.Pixel);
            
            return bitmap;
        }

        private static Image ClipImageToCircle(Image image)
        {
            Image destination = new Bitmap(image.Width, image.Height, image.PixelFormat);
            var radius = image.Width / 2;
            var x = image.Width / 2;
            var y = image.Height / 2;

            using Graphics g = Graphics.FromImage(destination);
            var r = new Rectangle(x - radius, y - radius, radius * 2, radius * 2);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (Brush brush = new SolidBrush(Color.Transparent))
            {
                g.FillRectangle(brush, 0, 0, destination.Width, destination.Height);
            }

            var path = new GraphicsPath();
            path.AddEllipse(r);
            g.SetClip(path);
            g.DrawImage(image, 0, 0);
            return destination;
        }

        private static Image CopyRegionIntoImage(Image source, Image destination)
        {
            using var grD = Graphics.FromImage(destination);
            var x = (destination.Width / 2) - 110;
            var y = (destination.Height / 2) - 155;

            grD.DrawImage(source, x, y, 220, 220);
            return destination;
        }
        
        private static Image DrawTextToImage(Image image, string header, string subheader)
        {
            var roboto = new Font(new FontFamily("Franklin Gothic Medium"), 30, FontStyle.Bold);
            var robotoSmall = new Font(new FontFamily("Franklin Gothic Medium"), 23, FontStyle.Regular);

            var brushWhite = new SolidBrush(Color.White);
            var brushGrey = new SolidBrush(Color.White);

            var headerX = image.Width / 2;
            var headerY = (image.Height / 2) + 115;

            var subheaderX = image.Width / 2;
            var subheaderY = (image.Height / 2) + 160;

            var drawFormat = new StringFormat
            {
                LineAlignment = StringAlignment.Center,
                Alignment = StringAlignment.Center
            };

            using var GrD = Graphics.FromImage(image);
            GrD.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            GrD.DrawString(header, roboto, brushWhite, headerX, headerY, drawFormat);
            GrD.DrawString(subheader, robotoSmall, brushGrey, subheaderX, subheaderY, drawFormat);

            var img = new Bitmap(image);
            return img;
        }

        private static async Task<Image> FetchImageAsync(string url = "https://pbs.twimg.com/media/D-zKA32UYAAujjV.jpg")
        {
            var client = new HttpClient();
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var backupResponse = await client.GetAsync(url);
                var backupStream = await backupResponse.Content.ReadAsStreamAsync();
                return Image.FromStream(backupStream);
            }

            var stream = await response.Content.ReadAsStreamAsync();
            return Image.FromStream(stream);
        }
    }
}
