using Unity.NetCode;
using Unity.Networking.Transport;

public struct PlayerInput : ICommandData<PlayerInput>
{
    public uint Tick => tick;
    public uint tick;
    public byte keyForwardPressed;
    public byte keyBackwardPressed;
    public byte keyTurnRightPressed;
    public byte keyTurnLeftPressed;

    public void Deserialize(uint tick, ref DataStreamReader reader)
    {
        this.tick = tick;
        keyForwardPressed = reader.ReadByte();
        keyBackwardPressed = reader.ReadByte();
        keyTurnRightPressed = reader.ReadByte();
        keyTurnLeftPressed = reader.ReadByte();
    }

    public void Serialize(ref DataStreamWriter writer)
    {
       writer.WriteByte(keyForwardPressed);
       writer.WriteByte(keyBackwardPressed);
       writer.WriteByte(keyTurnRightPressed);
       writer.WriteByte(keyTurnLeftPressed);
    }

    public void Deserialize(uint tick, ref DataStreamReader reader, PlayerInput baseline,
        NetworkCompressionModel compressionModel)
    {
        Deserialize(tick, ref reader);
    }

    public void Serialize(ref DataStreamWriter writer, PlayerInput baseline, NetworkCompressionModel compressionModel)
    {
        Serialize(ref writer);
    }
}

public class PlayerInputSendCommandSystem : CommandSendSystem<PlayerInput>
{
}
public class PlayerInputReceiveCommandSystem : CommandReceiveSystem<PlayerInput>
{
}