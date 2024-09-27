
using Telegram.Bot.Types.Enums;

namespace Telegram.Bot.Types
{
    internal class InputFiles
    {
        internal class InputOnlineFile : InputFile
        {
            private FileStream stream;
            private string v;
            private FileStream videoStream;

            public InputOnlineFile(FileStream videoStream)
            {
                this.videoStream = videoStream;
            }

            public InputOnlineFile(FileStream stream, string v)
            {
                this.stream = stream;
                this.v = v;
            }

            public override FileType FileType => throw new NotImplementedException();
        }
    }
}