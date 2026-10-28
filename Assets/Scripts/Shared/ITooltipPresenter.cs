public interface ITooltipPresenter
{
    void Show(TooltipDto dto);
    void Hide();
}

public static class TooltipPresenter
{
    public static ITooltipPresenter Current;
}