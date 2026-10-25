using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
namespace EasyRetro {
 internal static class Downloader {
  public static string Hash(string path) {using(SHA256 sha=SHA256.Create())using(FileStream stream=File.OpenRead(path))return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-","").ToLowerInvariant();}
  public static string Fetch(Dependency d,string cache) {
   ServicePointManager.SecurityProtocol=SecurityProtocolType.Tls12;Directory.CreateDirectory(cache);string path=Path.Combine(cache,d.Id+"-"+d.Version+".exe");
   if(File.Exists(path)) {if(Hash(path)!=d.Sha256)throw new InvalidDataException("SHA-256 불일치 (캐시 삭제 후 재시도): "+path);Log.Write(d.Id+" SHA-256 verified (cache): "+d.Sha256);return path;}
   string part=path+".download";try {using(WebClient client=new WebClient()) {client.Headers[HttpRequestHeader.UserAgent]="EasyRetro/0.1";client.DownloadFile(d.Url,part);}string actual=Hash(part);if(actual!=d.Sha256)throw new InvalidDataException(d.Id+" SHA-256 불일치: "+actual);File.Move(part,path);Log.Write(d.Id+" SHA-256 verified: "+actual);return path;}finally {if(File.Exists(part))File.Delete(part);}
  }
 }
}
