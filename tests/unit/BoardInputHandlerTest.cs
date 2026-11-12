using ClasicosMenteActiva.minigames.word_search;
using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class BoardInputHandlerTest
{
    [TestCase]
    public void TouchBeginUpdateEndEmiteEventos()
    {
        var handler = new BoardInputHandler();
        Vector2? began = null, updated = null, ended = null;
        var canceled = 0;
        handler.SelectionBegan += p => began = p;
        handler.SelectionUpdated += p => updated = p;
        handler.SelectionEnded += p => ended = p;
        handler.SelectionCanceled += () => canceled++;

        var down = new InputEventScreenTouch { Pressed = true, Position = new Vector2(10, 20) };
        AssertBool(handler.Process(down)).IsTrue();
        AssertBool(began.HasValue).IsTrue();
        AssertFloat(began.Value.X).IsEqual(10f);
        AssertFloat(began.Value.Y).IsEqual(20f);

        var drag = new InputEventScreenDrag { Position = new Vector2(12, 22) };
        AssertBool(handler.Process(drag)).IsTrue();
        AssertBool(updated.HasValue).IsTrue();

        var up = new InputEventScreenTouch { Pressed = false, Position = new Vector2(14, 24) };
        AssertBool(handler.Process(up)).IsTrue();
        AssertBool(ended.HasValue).IsTrue();
        AssertInt(canceled).IsEqual(0);

        // release sin pressed previo -> cancel
        var stray = new InputEventScreenTouch { Pressed = false, Position = Vector2.Zero };
        AssertBool(handler.Process(stray)).IsTrue();
        AssertInt(canceled).IsEqual(1);
    }

    [TestCase]
    public void MouseFlowYCancelManual()
    {
        var handler = new BoardInputHandler();
        var began = 0; var ended = 0; var canceled = 0; var updated = 0;
        handler.SelectionBegan += _ => began++;
        handler.SelectionEnded += _ => ended++;
        handler.SelectionCanceled += () => canceled++;
        handler.SelectionUpdated += _ => updated++;

        var down = new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = true, Position = new Vector2(5, 5) };
        AssertBool(handler.Process(down)).IsTrue();
        AssertInt(began).IsEqual(1);

        var motion = new InputEventMouseMotion { Position = new Vector2(6, 6) };
        AssertBool(handler.Process(motion)).IsTrue();
        AssertInt(updated).IsEqual(1);

        handler.Cancel();
        AssertInt(canceled).IsEqual(1);

        // motion sin pressed no consume
        var motion2 = new InputEventMouseMotion { Position = new Vector2(7, 7) };
        AssertBool(handler.Process(motion2)).IsFalse();

        // drag sin pressed no consume
        var drag = new InputEventScreenDrag { Position = new Vector2(1, 1) };
        AssertBool(handler.Process(drag)).IsFalse();

        // mouse release sin pressed previo
        var up = new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = false, Position = Vector2.Zero };
        AssertBool(handler.Process(up)).IsTrue();
        AssertInt(canceled).IsEqual(2);
    }

    [TestCase]
    public void EventosIrrelevantesNoSeConsumen()
    {
        var handler = new BoardInputHandler();
        var key = new InputEventKey { Pressed = true };
        AssertBool(handler.Process(key)).IsFalse();
        var right = new InputEventMouseButton { ButtonIndex = MouseButton.Right, Pressed = true };
        // right button is still a MouseButton but not Left in our switch? Process handles MouseButton Left only in pattern
        // actually pattern is InputEventMouseButton { ButtonIndex: MouseButton.Left } so right won't match that arm -> default? no other arms match -> returns false
        AssertBool(handler.Process(right)).IsFalse();
    }
}
