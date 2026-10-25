using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
namespace EasyRetro {
 internal static class Downloader {
  public static string Hash(string path){using(SHA256 sha=SHA256.Create())using(FileStream stream=File.OpenRead(path))return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-","").ToLowerInvariant();}
  public static string Fetch(Dependency dependency,string cache) {
   ServicePointManager.SecurityProtocol=SecurityProtocolType.Tls12;Directory.CreateDirectory(cache);
   string path=Path.Combine(cache,dependency.Id+"-"+dependency.Version+".exe"),part=path+".download";
   try {
    if(File.Exists(part))File.Delete(part);
    if(File.Exists(path)){if(Hash(path)!=dependency.Sha256)throw new InvalidDataException("SHA-256 불일치 (캐시 파일 삭제 후 재시도): "+path);Log.Write(dependency.Id+" SHA-256 verified (cache): "+dependency.Sha256);return path;}
    using(WebClient client=new WebClient()){client.Headers[HttpRequestHeader.UserAgent]="EasyRetro/0.1";client.DownloadFile(dependency.Url,part);}
    string actual=Hash(part);if(actual!=dependency.Sha256)throw new InvalidDataException(dependency.Id+" SHA-256 불일치: "+actual);
    File.Move(part,path);Log.Write(dependency.Id+" SHA-256 verified: "+actual);return path;
   }catch(WebException error){throw new InvalidOperationException("구성요소 다운로드에 실패했습니다. 인터넷 연결, 방화벽 및 GitHub 접속 가능 여부를 확인하고 다시 시도하세요. ("+dependency.Id+")",error);}
   finally{if(File.Exists(part))File.Delete(part);}
  }
 }
}
