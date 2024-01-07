// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

public interface IItemHoverHandler
{
    void OnHover();
    void OnHoverEnd();
}

public interface IItemSelectHandler
{
    bool OnSelected();
    void OnDeSelecded();
}

public interface IItemDoubleclickHandler
{
    void OnDoubleclick();
}