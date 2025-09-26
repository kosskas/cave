public interface IMode
{   
    PlayerController PCref { get; }
    void HandleInput();

    void SetUpFlystick();
}