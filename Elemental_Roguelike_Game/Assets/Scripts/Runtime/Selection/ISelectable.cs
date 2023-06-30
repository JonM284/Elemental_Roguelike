namespace Runtime.Selection
{
    public interface ISelectable
    {

        public void OnSelect();

        public void OnUnselected();

        public void OnHover();

        public void OnUnHover();

    }
}