using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using T0yK4T.Tools;

namespace T0yK4T.Sound
{
    ///// <summary>
    ///// Container for play completed event arguments
    ///// </summary>
    //public class ChannelEventArgs : EventArgs
    //{
    //    /// <summary>
    //    /// Gets the <see cref="ChannelContainer"/> that has finished playback
    //    /// </summary>
    //    public ChannelContainer Container { get; private set; }

    //    /// <summary>
    //    /// Initializes a new instance of <see cref="ChannelEventArgs"/> and sets the container property to the specified value
    //    /// </summary>
    //    /// <param name="container"></param>
    //    public ChannelEventArgs(ChannelContainer container)
    //    {
    //        this.Container = container;
    //    }
    //}

    /// <summary>
    /// A Wrapper class for the C# FMOD wrapper - A wrapper for a wrapper...
    /// </summary>
    public class FMODSystem
    {
        private TimeSpan pollingInterval;
        private FMOD.System system = null;
        private List<FMODChannel> playingChannels = new List<FMODChannel>();
        private Timer soundChecker;
        private bool destroyCalled = false;

        /// <summary>
        /// This event is fired when a channel / sound has finished playing - the ID of the channel is supplied through the arguments
        /// </summary>
        public event GenericEventHandler<FMODChannel> PlayComplete;

        /// <summary>
        /// This event is fired when a channel / sound changes "play" progress
        /// </summary>
        public event GenericEventHandler<FMODChannel> PlayProgress;

        /// <summary>
        /// Initializes a new instance of FMODSystem and starts an FMOD.System with the specified parameters
        /// </summary>
        /// <param name="channels">The number of channels</param>
        /// <param name="flags">The FMOD.INITFLAGS to pass to the system on initialization</param>
        /// <param name="pollingInterval">This parameter defines how often the system is polled for status</param>
        /// <param name="extraDriverData">The extra driver data to pass to the system on intialization</param>
        public FMODSystem(int channels, FMOD.INITFLAGS flags, IntPtr extraDriverData, TimeSpan pollingInterval)
        {
            if (pollingInterval.Ticks / TimeSpan.TicksPerMillisecond < 1)
                throw new ArgumentOutOfRangeException("PollingInterval must be a timespan of more than 0 milliseconds!");
            FMOD.RESULT result = FMOD.Factory.System_Create(ref this.system);
            if (result != FMOD.RESULT.OK)
                throw new FMODException("Unable to create FMOD System", result);
            result = this.system.init(channels, FMOD.INITFLAGS.NORMAL, extraDriverData);
            if (result != FMOD.RESULT.OK)
                throw new FMODException("Unable to Initialize FMOD System", result);
            this.pollingInterval = pollingInterval;
            this.soundChecker = new Timer(this.CheckChannels, null, this.pollingInterval.Ticks / TimeSpan.TicksPerMillisecond, Timeout.Infinite);
            
            //ServiceManager.RegisterService(this);
        }

        /// <summary>
        /// Destroys the system - Releasing resources
        /// </summary>
        public void Destroy()
        {
            if (!this.destroyCalled)
            {
                this.soundChecker.Change(Timeout.Infinite, Timeout.Infinite);
                this.destroyCalled = true;
                foreach (FMODChannel channel in this.playingChannels)
                    channel.StopAndRelease();
                this.playingChannels.Clear();
                this.playingChannels = null;
                this.system.close();
                this.system.release();
                this.system = null;
                GC.Collect();
            }
        }

        /// <summary>
        /// ...
        /// </summary>
        ~FMODSystem()
        {
            this.Destroy();
        }

        private void CheckChannels(object unset)
        {
            if (!this.destroyCalled)
            {
                lock (this.playingChannels)
                {
                    try
                    {
                        List<Guid> finished = new List<Guid>();
                        foreach (FMODChannel progressChannel in this.playingChannels)
                        {
                            if (progressChannel.Sound == null || progressChannel.CurrentPosition < progressChannel.Length)
                                finished.Add(progressChannel.ID);
                            else if (progressChannel.IsPlaying && (!progressChannel.IsPaused || progressChannel.CurrentPosition != progressChannel.LastPosition))
                                this.OnSoundProgress(progressChannel);
                        }
                        this.playingChannels.RemoveAll(c =>
                        {
                            if (finished.Contains(c.ID))
                            {
                                finished.Remove(c.ID);
                                return true;
                            }
                            else
                                return false;
                        });
                    }
                    catch (Exception er)
                    {
                        Console.WriteLine(er);
                    }

                    this.soundChecker.Change(this.pollingInterval.Ticks / TimeSpan.TicksPerMillisecond, Timeout.Infinite);
                }
            }
        }

        private void OnSoundProgress(FMODChannel channel)
        {
            if (this.PlayProgress != null)
                this.PlayProgress.BeginInvoke(channel, new AsyncCallback(iar => this.PlayProgress.EndInvoke(iar)), null);
        }

        private void OnSoundFinished(FMODChannel channel)
        {
            if (this.PlayComplete != null)
                this.PlayComplete.BeginInvoke(channel, new AsyncCallback(iar => this.PlayComplete.EndInvoke(iar)), null);
        }

        /// <summary>
        /// Removes the specified ChannelContainer and nulls it (for garbage collection)
        /// </summary>
        /// <param name="container"></param>
        internal void RemoveContainer(FMODChannel container)
        {
            this.playingChannels.RemoveAll(c => c.ID == container.ID);
        }

        /// <summary>
        /// Gets a value indicating how many channels are considered to be playing at this time
        /// </summary>
        public int PlayingCount
        {
            get { return this.playingChannels.Count; }
        }

        /// <summary>
        /// Calls <see cref="FMODSystem(int, FMOD.INITFLAGS, IntPtr, TimeSpan)"/> with the specified values and a <see cref="IntPtr.Zero"/> pointer
        /// </summary>
        /// <param name="channels"></param>
        /// <param name="flags"></param>
        /// <param name="pollingInterval"></param>
        public FMODSystem(int channels, FMOD.INITFLAGS flags, TimeSpan pollingInterval)
            : this(channels, flags, IntPtr.Zero, pollingInterval) { }

        /// <summary>
        /// Calls <see cref="FMODSystem(int, FMOD.INITFLAGS, TimeSpan)"/> with the timespan parameter set to a timespan of 50 milliseconds
        /// </summary>
        /// <param name="channels"></param>
        /// <param name="flags"></param>
        public FMODSystem(int channels, FMOD.INITFLAGS flags)
            : this(channels, flags, TimeSpan.FromMilliseconds(50)) { }

        private FMOD.Channel PlaySound(FMOD.Sound sound, bool play)
        {
            FMOD.Channel channel = null;
            FMOD.RESULT result = this.system.playSound(FMOD.CHANNELINDEX.FREE, sound, !play, ref channel);
            if (result != FMOD.RESULT.OK)
                throw new FMODException("Unable to play sound", result);
            return channel;
        }

        /// <summary>
        /// Attempts to load the file specified by <paramref name="file"/> and play it on the system
        /// </summary>
        /// <param name="file">The full path to the file that should be played</param>
        /// <returns></returns>
        public FMODChannel PlayFile(string file)
        {
            return this.PlayFile(file, true);
        }

        private FMODChannel PlayFile(string file, bool play)
        {
            if (string.IsNullOrEmpty(file) || !System.IO.File.Exists(file))
                throw new System.IO.FileNotFoundException("Can't find file", file ?? string.Empty);
            FMOD.Sound sound = this.CreateSoundFromFile(file);

            FMODChannel container = new FMODChannel(this, this.PlaySound(sound, play), Guid.NewGuid());
            lock (this.playingChannels)
                this.playingChannels.Add(container);
            return container;
        }

        /// <summary>
        /// Loads the specified file and creates a channel for it, but does not play it yet
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public FMODChannel PreloadFile(string file)
        {
            return this.PlayFile(file, false);
        }

        private FMOD.Sound CreateSoundFromFile(string file)
        {
            if (string.IsNullOrEmpty(file) || !System.IO.File.Exists(file))
                throw new System.IO.FileNotFoundException("Can't find file", file ?? string.Empty);
            FMOD.Sound sound = null;
            FMOD.RESULT result = this.system.createSound(file, FMOD.MODE.CREATESTREAM, ref sound);
            if (result != FMOD.RESULT.OK)
                throw new FMODException("Unable to create sound", result);
            return sound;
        }
    }
}
