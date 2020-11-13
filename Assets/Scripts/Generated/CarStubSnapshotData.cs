using Unity.Networking.Transport;
using Unity.NetCode;
using Unity.Mathematics;

public struct CarStubSnapshotData : ISnapshotData<CarStubSnapshotData>
{
    public uint tick;
    private int HealthComponentHealth;
    private uint ProgressionComponentCrossedCheckpoints;
    private int SynchronizedCarComponentPlayerId;
    private int SynchronizedCarComponentSteerAngle;
    private uint SynchronizedCarComponentIsShieldActive;
    private int RotationValueX;
    private int RotationValueY;
    private int RotationValueZ;
    private int RotationValueW;
    private int TranslationValueX;
    private int TranslationValueY;
    private int TranslationValueZ;
    uint changeMask0;

    public uint Tick => tick;
    public float GetHealthComponentHealth(GhostDeserializerState deserializerState)
    {
        return HealthComponentHealth * 0.1f;
    }
    public float GetHealthComponentHealth()
    {
        return HealthComponentHealth * 0.1f;
    }
    public void SetHealthComponentHealth(float val, GhostSerializerState serializerState)
    {
        HealthComponentHealth = (int)(val * 10);
    }
    public void SetHealthComponentHealth(float val)
    {
        HealthComponentHealth = (int)(val * 10);
    }
    public uint GetProgressionComponentCrossedCheckpoints(GhostDeserializerState deserializerState)
    {
        return (uint)ProgressionComponentCrossedCheckpoints;
    }
    public uint GetProgressionComponentCrossedCheckpoints()
    {
        return (uint)ProgressionComponentCrossedCheckpoints;
    }
    public void SetProgressionComponentCrossedCheckpoints(uint val, GhostSerializerState serializerState)
    {
        ProgressionComponentCrossedCheckpoints = (uint)val;
    }
    public void SetProgressionComponentCrossedCheckpoints(uint val)
    {
        ProgressionComponentCrossedCheckpoints = (uint)val;
    }
    public int GetSynchronizedCarComponentPlayerId(GhostDeserializerState deserializerState)
    {
        return (int)SynchronizedCarComponentPlayerId;
    }
    public int GetSynchronizedCarComponentPlayerId()
    {
        return (int)SynchronizedCarComponentPlayerId;
    }
    public void SetSynchronizedCarComponentPlayerId(int val, GhostSerializerState serializerState)
    {
        SynchronizedCarComponentPlayerId = (int)val;
    }
    public void SetSynchronizedCarComponentPlayerId(int val)
    {
        SynchronizedCarComponentPlayerId = (int)val;
    }
    public float GetSynchronizedCarComponentSteerAngle(GhostDeserializerState deserializerState)
    {
        return SynchronizedCarComponentSteerAngle * 0.1f;
    }
    public float GetSynchronizedCarComponentSteerAngle()
    {
        return SynchronizedCarComponentSteerAngle * 0.1f;
    }
    public void SetSynchronizedCarComponentSteerAngle(float val, GhostSerializerState serializerState)
    {
        SynchronizedCarComponentSteerAngle = (int)(val * 10);
    }
    public void SetSynchronizedCarComponentSteerAngle(float val)
    {
        SynchronizedCarComponentSteerAngle = (int)(val * 10);
    }
    public bool GetSynchronizedCarComponentIsShieldActive(GhostDeserializerState deserializerState)
    {
        return SynchronizedCarComponentIsShieldActive!=0;
    }
    public bool GetSynchronizedCarComponentIsShieldActive()
    {
        return SynchronizedCarComponentIsShieldActive!=0;
    }
    public void SetSynchronizedCarComponentIsShieldActive(bool val, GhostSerializerState serializerState)
    {
        SynchronizedCarComponentIsShieldActive = val?1u:0;
    }
    public void SetSynchronizedCarComponentIsShieldActive(bool val)
    {
        SynchronizedCarComponentIsShieldActive = val?1u:0;
    }
    public quaternion GetRotationValue(GhostDeserializerState deserializerState)
    {
        return GetRotationValue();
    }
    public quaternion GetRotationValue()
    {
        return new quaternion(RotationValueX * 0.0001f, RotationValueY * 0.0001f, RotationValueZ * 0.0001f, RotationValueW * 0.0001f);
    }
    public void SetRotationValue(quaternion q, GhostSerializerState serializerState)
    {
        SetRotationValue(q);
    }
    public void SetRotationValue(quaternion q)
    {
        RotationValueX = (int)(q.value.x * 10000);
        RotationValueY = (int)(q.value.y * 10000);
        RotationValueZ = (int)(q.value.z * 10000);
        RotationValueW = (int)(q.value.w * 10000);
    }
    public float3 GetTranslationValue(GhostDeserializerState deserializerState)
    {
        return GetTranslationValue();
    }
    public float3 GetTranslationValue()
    {
        return new float3(TranslationValueX * 0.0001f, TranslationValueY * 0.0001f, TranslationValueZ * 0.0001f);
    }
    public void SetTranslationValue(float3 val, GhostSerializerState serializerState)
    {
        SetTranslationValue(val);
    }
    public void SetTranslationValue(float3 val)
    {
        TranslationValueX = (int)(val.x * 10000);
        TranslationValueY = (int)(val.y * 10000);
        TranslationValueZ = (int)(val.z * 10000);
    }

    public void PredictDelta(uint tick, ref CarStubSnapshotData baseline1, ref CarStubSnapshotData baseline2)
    {
        var predictor = new GhostDeltaPredictor(tick, this.tick, baseline1.tick, baseline2.tick);
        HealthComponentHealth = predictor.PredictInt(HealthComponentHealth, baseline1.HealthComponentHealth, baseline2.HealthComponentHealth);
        ProgressionComponentCrossedCheckpoints = (uint)predictor.PredictInt((int)ProgressionComponentCrossedCheckpoints, (int)baseline1.ProgressionComponentCrossedCheckpoints, (int)baseline2.ProgressionComponentCrossedCheckpoints);
        SynchronizedCarComponentPlayerId = predictor.PredictInt(SynchronizedCarComponentPlayerId, baseline1.SynchronizedCarComponentPlayerId, baseline2.SynchronizedCarComponentPlayerId);
        SynchronizedCarComponentSteerAngle = predictor.PredictInt(SynchronizedCarComponentSteerAngle, baseline1.SynchronizedCarComponentSteerAngle, baseline2.SynchronizedCarComponentSteerAngle);
        SynchronizedCarComponentIsShieldActive = (uint)predictor.PredictInt((int)SynchronizedCarComponentIsShieldActive, (int)baseline1.SynchronizedCarComponentIsShieldActive, (int)baseline2.SynchronizedCarComponentIsShieldActive);
        RotationValueX = predictor.PredictInt(RotationValueX, baseline1.RotationValueX, baseline2.RotationValueX);
        RotationValueY = predictor.PredictInt(RotationValueY, baseline1.RotationValueY, baseline2.RotationValueY);
        RotationValueZ = predictor.PredictInt(RotationValueZ, baseline1.RotationValueZ, baseline2.RotationValueZ);
        RotationValueW = predictor.PredictInt(RotationValueW, baseline1.RotationValueW, baseline2.RotationValueW);
        TranslationValueX = predictor.PredictInt(TranslationValueX, baseline1.TranslationValueX, baseline2.TranslationValueX);
        TranslationValueY = predictor.PredictInt(TranslationValueY, baseline1.TranslationValueY, baseline2.TranslationValueY);
        TranslationValueZ = predictor.PredictInt(TranslationValueZ, baseline1.TranslationValueZ, baseline2.TranslationValueZ);
    }

    public void Serialize(int networkId, ref CarStubSnapshotData baseline, ref DataStreamWriter writer, NetworkCompressionModel compressionModel)
    {
        changeMask0 = (HealthComponentHealth != baseline.HealthComponentHealth) ? 1u : 0;
        changeMask0 |= (ProgressionComponentCrossedCheckpoints != baseline.ProgressionComponentCrossedCheckpoints) ? (1u<<1) : 0;
        changeMask0 |= (SynchronizedCarComponentPlayerId != baseline.SynchronizedCarComponentPlayerId) ? (1u<<2) : 0;
        changeMask0 |= (SynchronizedCarComponentSteerAngle != baseline.SynchronizedCarComponentSteerAngle) ? (1u<<3) : 0;
        changeMask0 |= (SynchronizedCarComponentIsShieldActive != baseline.SynchronizedCarComponentIsShieldActive) ? (1u<<4) : 0;
        changeMask0 |= (RotationValueX != baseline.RotationValueX ||
                                           RotationValueY != baseline.RotationValueY ||
                                           RotationValueZ != baseline.RotationValueZ ||
                                           RotationValueW != baseline.RotationValueW) ? (1u<<5) : 0;
        changeMask0 |= (TranslationValueX != baseline.TranslationValueX ||
                                           TranslationValueY != baseline.TranslationValueY ||
                                           TranslationValueZ != baseline.TranslationValueZ) ? (1u<<6) : 0;
        writer.WritePackedUIntDelta(changeMask0, baseline.changeMask0, compressionModel);
        if ((changeMask0 & (1 << 0)) != 0)
            writer.WritePackedIntDelta(HealthComponentHealth, baseline.HealthComponentHealth, compressionModel);
        if ((changeMask0 & (1 << 1)) != 0)
            writer.WritePackedUIntDelta(ProgressionComponentCrossedCheckpoints, baseline.ProgressionComponentCrossedCheckpoints, compressionModel);
        if ((changeMask0 & (1 << 2)) != 0)
            writer.WritePackedIntDelta(SynchronizedCarComponentPlayerId, baseline.SynchronizedCarComponentPlayerId, compressionModel);
        if ((changeMask0 & (1 << 3)) != 0)
            writer.WritePackedIntDelta(SynchronizedCarComponentSteerAngle, baseline.SynchronizedCarComponentSteerAngle, compressionModel);
        if ((changeMask0 & (1 << 4)) != 0)
            writer.WritePackedUIntDelta(SynchronizedCarComponentIsShieldActive, baseline.SynchronizedCarComponentIsShieldActive, compressionModel);
        if ((changeMask0 & (1 << 5)) != 0)
        {
            writer.WritePackedIntDelta(RotationValueX, baseline.RotationValueX, compressionModel);
            writer.WritePackedIntDelta(RotationValueY, baseline.RotationValueY, compressionModel);
            writer.WritePackedIntDelta(RotationValueZ, baseline.RotationValueZ, compressionModel);
            writer.WritePackedIntDelta(RotationValueW, baseline.RotationValueW, compressionModel);
        }
        if ((changeMask0 & (1 << 6)) != 0)
        {
            writer.WritePackedIntDelta(TranslationValueX, baseline.TranslationValueX, compressionModel);
            writer.WritePackedIntDelta(TranslationValueY, baseline.TranslationValueY, compressionModel);
            writer.WritePackedIntDelta(TranslationValueZ, baseline.TranslationValueZ, compressionModel);
        }
    }

    public void Deserialize(uint tick, ref CarStubSnapshotData baseline, ref DataStreamReader reader,
        NetworkCompressionModel compressionModel)
    {
        this.tick = tick;
        changeMask0 = reader.ReadPackedUIntDelta(baseline.changeMask0, compressionModel);
        if ((changeMask0 & (1 << 0)) != 0)
            HealthComponentHealth = reader.ReadPackedIntDelta(baseline.HealthComponentHealth, compressionModel);
        else
            HealthComponentHealth = baseline.HealthComponentHealth;
        if ((changeMask0 & (1 << 1)) != 0)
            ProgressionComponentCrossedCheckpoints = reader.ReadPackedUIntDelta(baseline.ProgressionComponentCrossedCheckpoints, compressionModel);
        else
            ProgressionComponentCrossedCheckpoints = baseline.ProgressionComponentCrossedCheckpoints;
        if ((changeMask0 & (1 << 2)) != 0)
            SynchronizedCarComponentPlayerId = reader.ReadPackedIntDelta(baseline.SynchronizedCarComponentPlayerId, compressionModel);
        else
            SynchronizedCarComponentPlayerId = baseline.SynchronizedCarComponentPlayerId;
        if ((changeMask0 & (1 << 3)) != 0)
            SynchronizedCarComponentSteerAngle = reader.ReadPackedIntDelta(baseline.SynchronizedCarComponentSteerAngle, compressionModel);
        else
            SynchronizedCarComponentSteerAngle = baseline.SynchronizedCarComponentSteerAngle;
        if ((changeMask0 & (1 << 4)) != 0)
            SynchronizedCarComponentIsShieldActive = reader.ReadPackedUIntDelta(baseline.SynchronizedCarComponentIsShieldActive, compressionModel);
        else
            SynchronizedCarComponentIsShieldActive = baseline.SynchronizedCarComponentIsShieldActive;
        if ((changeMask0 & (1 << 5)) != 0)
        {
            RotationValueX = reader.ReadPackedIntDelta(baseline.RotationValueX, compressionModel);
            RotationValueY = reader.ReadPackedIntDelta(baseline.RotationValueY, compressionModel);
            RotationValueZ = reader.ReadPackedIntDelta(baseline.RotationValueZ, compressionModel);
            RotationValueW = reader.ReadPackedIntDelta(baseline.RotationValueW, compressionModel);
        }
        else
        {
            RotationValueX = baseline.RotationValueX;
            RotationValueY = baseline.RotationValueY;
            RotationValueZ = baseline.RotationValueZ;
            RotationValueW = baseline.RotationValueW;
        }
        if ((changeMask0 & (1 << 6)) != 0)
        {
            TranslationValueX = reader.ReadPackedIntDelta(baseline.TranslationValueX, compressionModel);
            TranslationValueY = reader.ReadPackedIntDelta(baseline.TranslationValueY, compressionModel);
            TranslationValueZ = reader.ReadPackedIntDelta(baseline.TranslationValueZ, compressionModel);
        }
        else
        {
            TranslationValueX = baseline.TranslationValueX;
            TranslationValueY = baseline.TranslationValueY;
            TranslationValueZ = baseline.TranslationValueZ;
        }
    }
    public void Interpolate(ref CarStubSnapshotData target, float factor)
    {
        SetRotationValue(math.slerp(GetRotationValue(), target.GetRotationValue(), factor));
        SetTranslationValue(math.lerp(GetTranslationValue(), target.GetTranslationValue(), factor));
    }
}
