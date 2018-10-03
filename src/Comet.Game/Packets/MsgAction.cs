namespace Comet.Game.Packets
{
    using System;
    using Comet.Game.States;
    using Comet.Network.Packets;

    /// <remarks>Packet Type 1010</remarks>
    /// <summary>
    /// Message containing a general action being performed by the client. Commonly used
    /// as a request-response protocol for question and answer like exchanges. For example,
    /// walk requests are responded to with an answer as to if the step is legal or not.
    /// </summary>
    public sealed class MsgAction : MsgBase<Client>
    {
        // Packet Properties
        public uint Timestamp { get; set; }
        public uint CharacterID { get; set; }
        public uint Command { get; set; }
        public ushort[] Arguments { get; set; }
        public ushort Direction { get; set; }
        public ActionType Action { get; set; }

        /// <summary>
        /// Decodes a byte packet into the packet structure defined by this message class. 
        /// Should be invoked to structure data from the client for processing. Decoding
        /// follows TQ Digital's byte ordering rules for an all-binary protocol.
        /// </summary>
        /// <param name="bytes">Bytes from the packet processor or client socket</param>
        public override void Decode(byte[] bytes)
        {
            var reader = new PacketReader(bytes);
            this.Length = reader.ReadUInt16();
            this.Type = (PacketType)reader.ReadUInt16();
            this.Timestamp = reader.ReadUInt32();
            this.CharacterID = reader.ReadUInt32();
            this.Command = reader.ReadUInt32();
            this.Arguments = new ushort[2];
            for (int i = 0; i < this.Arguments.Length; i++)
                this.Arguments[i] = reader.ReadUInt16();
            this.Direction = reader.ReadUInt16();
            this.Action = (ActionType)reader.ReadUInt16();
        }

        /// <summary>
        /// Encodes the packet structure defined by this message class into a byte packet
        /// that can be sent to the client. Invoked automatically by the client's send 
        /// method. Encodes using byte ordering rules interoperable with the game client.
        /// </summary>
        /// <returns>Returns a byte packet of the encoded packet.</returns>
        public override byte[] Encode()
        {
            var writer = new PacketWriter();
            writer.Write((ushort)base.Type);
            writer.Write(this.Timestamp);
            writer.Write(this.CharacterID);
            writer.Write(this.Command);
            for (int i = 0; i < this.Arguments.Length; i++)
                writer.Write(this.Arguments[i]);
            writer.Write(this.Direction);
            writer.Write((ushort)this.Action);
            return writer.ToArray();
        }

        /// <summary>
        /// Process can be invoked by a packet after decode has been called to structure
        /// packet fields and properties. For the server implementations, this is called
        /// in the packet handler after the message has been dequeued from the server's
        /// <see cref="PacketProcessor"/>.
        /// </summary>
        /// <param name="client">Client requesting packet processing</param>
        public override void Process(Client client)
        {
            switch (this.Action)
            {
                case ActionType.SetLocation:
                    this.CharacterID = client.Character.CharacterID;
                    this.Command = client.Character.MapID;
                    this.Arguments[0] = client.Character.X;
                    this.Arguments[1] = client.Character.Y;
                    client.Send(this);
                    break;

                default:
                    client.Send(this);
                    Console.WriteLine(
                        "Missing packet {0}, Length {1}\n{2}", 
                        this.Type, this.Length, PacketDump.Hex(this.Encode()));
                    break;
            }
        }

        /// <summary>
        /// Defines actions that may be requested by the user, or given to by the server.
        /// Allows for action handling as a packet subtype.
        /// </summary>
        public enum ActionType
        {
            SetLocation = 74,
            SetInventory = 75,
            SetAssociates = 76,
            SetProficiencies = 77,
            SetMagicSpells = 78,
            SetDirection = 79,
            SetAction = 80
        }
    }
}