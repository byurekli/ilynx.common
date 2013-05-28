using System.Threading;
using System;

namespace T0yK4T.Sound
{
    /// <summary>
    /// Container class for managing a single channel accross threads (Used during polling)
    /// </summary>
    public class FMODChannel
    {
        private Guid id;
        private FMOD.Sound sound;

        private FMODSystem system;
        private bool isDisposed = false;

        /// <summary>
        /// Gets the Channel that this <see cref="FMODChannel"/> is associated with
        /// </summary>
        public FMOD.Channel Channel { get; private set; }

        /// <summary>
        /// Gets the FMOD.Sound object that this <see cref="FMODChannel"/> is associated with
        /// </summary>
        public FMOD.Sound Sound { get { return this.sound; } }

        /// <summary>
        /// Gets the Last set position (in milliseconds) of this <see cref="FMODChannel"/>
        /// <para/>
        /// Only used externally
        /// </summary>
        public uint LastPosition
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the Length component (in milliseconds) of this <see cref="FMODChannel"/>
        /// </summary>
        public uint Length
        {
            get
            {
                return this.GetLength();
            }
        }

        private uint currentPosition;
        /// <summary>
        /// Gets the Current Position (in milliseconds) of this <see cref="FMODChannel"/>
        /// </summary>
        public uint CurrentPosition
        {
            get
            {
                this.currentPosition = this.GetPosition();
                return this.currentPosition;
            }
        }

        /// <summary>
        /// Gets the ID of this channel
        /// </summary>
        public Guid ID
        {
            get { return this.id; }
        }

        /// <summary>
        /// Initializes a new <see cref="FMODChannel"/> and sets it's values to the ones specified
        /// </summary>
        /// <param name="channel">The FMOD.Channel to wrap around</param>
        /// <param name="sound">The FMOD.Sound to wrap</param>
        /// <param name="context">The SynchronizationContext to use for callbacks</param>
        /// <param name="id">the ID of this container</param>
        /// <param name="system">The <see cref="FMODSystem"/> that contains this channel</param>
        internal FMODChannel(FMODSystem system, FMOD.Channel channel, FMOD.Sound sound, Guid id)
        {
            this.system = system;
            this.id = id;
            this.Channel = channel;
            this.sound = sound;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="FMODChannel"/> and attempts to load all other values automatically
        /// <para/>
        /// The <see cref="FMODChannel.Sound"/> property is set to the one found via FMOD.Channel.getCurrentSound(ref FMOD.Sound)
        /// <para/>
        /// The <see cref="FMODChannel.Context"/> property is set to either the current context, or a new SynchronizationContext (if the current is not valid)
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="id">The id of the channel</param>
        /// <param name="system">The <see cref="FMODSystem"/> that contains this channel</param>
        internal FMODChannel(FMODSystem system, FMOD.Channel channel, Guid id)
        {
            this.system = system;
            this.id = id;
            this.Channel = channel;
            FMOD.RESULT result = channel.getCurrentSound(ref this.sound);
            if (result != FMOD.RESULT.OK)
                throw new FMODException("Unable to get current sound for Container", result);
        }

        private object p_Lock = new object();

        private uint GetLength()
        {
            uint length = 0;
            if (!this.isDisposed)
            {
                lock (this.p_Lock)
                {
                    FMOD.RESULT result = this.sound.getLength(ref length, FMOD.TIMEUNIT.MS);
                    if (result != FMOD.RESULT.OK)
                        throw new FMODException("Unable to get sound length", result);
                }
            }
            return length;
        }

        private uint GetPosition()
        {
            uint position = 0;
            if (!this.isDisposed)
            {
                lock (this.p_Lock)
                {
                    FMOD.RESULT result = this.Channel.getPosition(ref position, FMOD.TIMEUNIT.MS);
                    if (result == FMOD.RESULT.ERR_CHANNEL_STOLEN)
                        return 0;
                    if (result != FMOD.RESULT.OK)
                        throw new FMODException("Unable to get channel position", result);
                }
            }
            return position;
        }

        /// <summary>
        /// Attempts to suspend playback on the current channel
        /// </summary>
        public void Pause()
        {
            if (!this.isDisposed)
            {
                lock (this.p_Lock)
                    this.SetPaused(true);
            }
        }

        /// <summary>
        /// Attempts to resume playback on the current channel
        /// </summary>
        public void Resume()
        {
            if (!this.isDisposed)
            {
                lock (this.p_Lock)
                    this.SetPaused(false);
            }
        }

        /// <summary>
        /// Alias for <see cref="FMODChannel.Resume"/>
        /// </summary>
        public void Play()
        {
            if (!this.isDisposed)
            {
                lock (this.p_Lock)
                    this.Resume();
            }
        }

        /// <summary>
        /// Attempts to stop playback on this channel, and release it
        /// </summary>
        public void StopAndRelease()
        {
            lock (this.p_Lock)
            {
                if (!this.isDisposed)
                {
                    this.isDisposed = true;
                    FMOD.Sound currentSound = null;

                    FMOD.RESULT result = this.Channel.getCurrentSound(ref currentSound);
                    if (result != FMOD.RESULT.OK)
                        throw new FMODException("Unable to retrieve current sound from channel", result);

                    result = currentSound.release();
                    if (result != FMOD.RESULT.OK)
                        throw new FMODException("Unable to release sound", result);
                    result = this.Channel.stop();
                    if (result != FMOD.RESULT.OK && result != FMOD.RESULT.ERR_INVALID_HANDLE) // FMOD.RESULT.ERR_INVALID_HANDLE usually means that the channel hasn't even started playing yet
                        throw new FMODException("Unable to release channel", result);
                    this.system.RemoveContainer(this);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether or not the current channel is actively playing (this can be true even if the channel is paused)
        /// </summary>
        public bool IsPlaying
        {
            get
            {
                return this.GetIsPlaying();
            }
        }

        /// <summary>
        /// Gets a value indicating whether or not the current channel is paused
        /// </summary>
        public bool IsPaused
        {
            get { return this.GetPaused(); }
        }

        private bool GetPaused()
        {
            bool ret = false;
            lock (this.p_Lock)
            {
                if (!this.isDisposed)
                {
                    FMOD.RESULT result = this.Channel.getPaused(ref ret);
                    if (result != FMOD.RESULT.OK)
                        throw new FMODException("Unable to get state", result);
                }
            }
            return ret;
        }

        private bool GetIsPlaying()
        {
            bool ret = false;
            if (!this.isDisposed)
            {
                lock (this.p_Lock)
                {
                    FMOD.RESULT result = this.Channel.isPlaying(ref ret);
                    if (result != FMOD.RESULT.OK)
                        throw new FMODException("Unable to get state", result);
                }
            }
            return ret;
        }

        private void SetPaused(bool paused)
        {
            if (!this.isDisposed)
            {
                lock (this.p_Lock)
                {
                    FMOD.RESULT result = this.Channel.setPaused(paused);
                    if (result != FMOD.RESULT.OK && result != FMOD.RESULT.ERR_INVALID_HANDLE)
                        throw new FMODException("Unable to pause the channel", result);
                }
            }
        }

        /// <summary>
        /// Attempts to seek to the specified position
        /// </summary>
        /// <param name="position">The desired offset</param>
        public void SetPosition(TimeSpan position)
        {
            if (!this.isDisposed)
            {
                lock (this.p_Lock)
                {
                    FMOD.RESULT result = this.Channel.setPosition(((uint)position.TotalMilliseconds), FMOD.TIMEUNIT.MS);
                    if (result != FMOD.RESULT.OK)
                        throw new FMODException("Unable to set playback position of channel", result);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether or not <see cref="FMODChannel.StopAndRelease()"/> has been called on this channelcontainer
        /// </summary>
        public bool IsDisposed
        {
            get { return this.isDisposed; }
        }
    }
}