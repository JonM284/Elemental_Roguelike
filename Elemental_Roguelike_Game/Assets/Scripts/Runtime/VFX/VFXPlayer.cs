using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Runtime.VFX
{
    public class VFXPlayer: MonoBehaviour
    {

        #region Public Fields

        public List<ParticleSystem> particleSystems = new List<ParticleSystem>();

        #endregion

        #region Serialized Fields

        [SerializeField] private string m_vfx_player_identifier;

        #endregion

        #region Accessors

        public bool is_playing => particleSystems.TrueForAll(ps => ps.isPlaying);

        public string vfxplayerIdentifier => m_vfx_player_identifier;

        #endregion

        #region Class Implementation

        //Play all VFX on this object
        public void Play()
        {
            particleSystems.ForEach(ps => ps.Play());
        }

        //Stop all VFX on this object
        public void Stop()
        {
            particleSystems.ForEach(ps => ps.Stop());
        }

        #endregion

    }
}