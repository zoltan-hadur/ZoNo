﻿namespace ZoNo.Contracts.Services
{
  public interface IPageService
  {
    Type GetPageType(string key);
  }

  public interface ITopLevelPageService : IPageService
  {

  }
}