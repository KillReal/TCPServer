using System;
using System.Collections.Generic;
using System.Text;
using Server.PocketFramework;

namespace Server.Pockets
{
    class ErrorPocket : BasePocket
    { 
        public int idError { get; set; }
        public string ErrorMessage { get; set; }

        public ErrorPocket(int idError, string ErrorMessage)
        {
            this.idError = idError;
            this.ErrorMessage = ErrorMessage;
        }

        public override byte[] ToBytes()
        {
            PocketConstructor pc = new PocketConstructor();
            pc.WriteInt32(idError);
            pc.WriteString(ErrorMessage);
            return pc.GetBytes();
        }

        public static ErrorPocket FromBytes(byte[] data)
        {
            PocketConstructor pc = new PocketConstructor(data);
            return new ErrorPocket(pc.ReadInt32(), pc.ReadString());
        }

        public override int GetType()
        {
            return (int)PocketEnum.Error;
        }
    }
}
