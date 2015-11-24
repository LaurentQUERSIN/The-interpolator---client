using UnityEngine;
using System.Collections;
using System.IO;
using System;

public class LocalPlayer : Stormancer.SynchBehaviourBase
{
    public Stormancer.RemoteScene Scene;
    private Stormancer.IClock Clock;
    private Rigidbody PlayerRigidbody;

    void OnAwake()
    {
        Clock = Scene.Scene.DependencyResolver.GetComponent<Stormancer.IClock>();
        PlayerRigidbody = this.GetComponent<Rigidbody>();
    }

    public override void SendChanges(Stream stream)
    {
        using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8))
        {
            writer.Write(Clock.Clock);
            writer.Write(this.transform.position.x);
            writer.Write(this.transform.position.y);
            writer.Write(this.transform.position.z);

            writer.Write(PlayerRigidbody.velocity.x * 1000);
            writer.Write(PlayerRigidbody.velocity.y * 1000);
            writer.Write(PlayerRigidbody.velocity.z * 1000);

            writer.Write(this.transform.rotation.x);
            writer.Write(this.transform.rotation.y);
            writer.Write(this.transform.rotation.z);
            writer.Write(this.transform.rotation.w);
        }
    }

    public override void ApplyChanges(Stream stream)
    {
        using (var reader = new BinaryReader(stream))
        {
            var stamp = reader.ReadInt64();
            var x = reader.ReadSingle();
            var y = reader.ReadSingle();
            var z = reader.ReadSingle();

            var vx = reader.ReadSingle();
            var vy = reader.ReadSingle();
            var vz = reader.ReadSingle();

            var rx = reader.ReadSingle();
            var ry = reader.ReadSingle();
            var rz = reader.ReadSingle();
            var rw = reader.ReadSingle();

            if (LastChanged < stamp)
            {
                LastChanged = stamp;
                Stormancer.MainThread.Post(() =>
                {
                    this.transform.position = new Vector3(x, y, z);
                    PlayerRigidbody.velocity = new Vector3(vx, vy, vz);
                    this.transform.rotation = new Quaternion(rx, ry, rz, rw);
                });
            }
        }
    }
}
