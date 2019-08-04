namespace Cratesmith.Utils
{
    public struct GossipReporting<TSelf> : IGossipContainer<TSelf>
    {
        public GossipReporting(TSelf _self) : this()
        {
            m_Self = _self;
            m_PausedFor = new PreallocLinkList<object>();
        }

        public event System.Action<TSelf> OnChanged;

        private PreallocLinkList<object> m_PausedFor;
        private bool m_ChangePending;
        private TSelf m_Self;
    
        public void Pause(object _pauseFor)
        {
            m_PausedFor.Add(_pauseFor);
        }

        public void Resume(object _resumeFor)
        {
            m_PausedFor.Remove(_resumeFor);
            if (IsPaused || !m_ChangePending)
            {
                return;
            }

            m_ChangePending = false;
            ++ChangeCount;
            OnChanged?.Invoke(m_Self);
        }

        public void MarkChanged()
        {
            if (!IsPaused)
            {
                ++ChangeCount;
                OnChanged?.Invoke(m_Self);
            }
            else
            {
                m_ChangePending = true;
            }
        }

        public bool IsPaused => m_PausedFor.Count > 0;
        public long ChangeCount { get; private set; }
    }
}