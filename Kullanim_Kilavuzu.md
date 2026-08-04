# Best Makina — DXF Analiz ve Reçete Aracı
## Kullanım Kılavuzu

> Bu belgeye ekran görüntülerinizi eklerken `[EKRAN GÖRÜNTÜSÜ: …]` satırlarını gerçek görsellerle değiştirin.

---

## 1. Uygulama hakkında

**Best Makina — DXF Analiz ve Reçete Aracı**, cam / profil işleme için hazırlanan DXF çizimlerini analiz eder, kenar uzunluklarını ve radius bilgilerini çıkarır, reçete oluşturur ve makineye **CSV / DAT** olarak kaydeder veya **FTP** ile gönderir.

**Temel iş akışı:**

1. Ayarları yapın (makine yönü, maksimum şekil ölçüleri, isteğe bağlı FTP)
2. DXF dosyası yükleyin
3. Taban kenarını (L1) seçin
4. Reçeteye ekleyin (kalınlık, offset, taş, cam kalınlığı, adet)
5. CSV / DAT çıktı alın veya FTP ile makineye gönderin

İsteğe bağlı olarak taş işleme **simülasyonu** ile planı doğrulayabilirsiniz.

---

## 2. Ana ekran

Uygulama açıldığında ana pencere dört bölgeden oluşur:

| Bölge | Ne işe yarar |
|--------|----------------|
| **Üst araç çubuğu** | Dosya seçme, reçete, simülasyon, çıktı ve FTP işlemleri |
| **Sol alan** | DXF çizimi (veya karşılama ekranı) |
| **Sağ üst** | Reçete listesi |
| **Sağ alt** | Şekil analizi (kenar uzunlukları, menfez, radius) |
| **Alt durum çubuğu** | İstatistik ve kısa durum mesajları |

**Üst araç çubuğu butonları (soldan sağa):**

- Dosya Seç
- CSV İçe Aktar
- Taban Kenarı Seç
- Reçeteye Ekle
- Simülasyon
- Toplu CSV Çıktı
- Toplu DAT Çıktı
- FTP ile Gönder
- Ayarlar (sağda)

`[EKRAN GÖRÜNTÜSÜ: Ana ekran — boş / hoş geldiniz hali]`

### 2.1 Hoş geldiniz ekranı

Henüz DXF veya reçete yokken sol alanda **Hoş geldiniz** paneli görünür:

- Başlık: Hoş geldiniz
- Açıklama: DXF yükleyin veya CSV içe aktarın
- Adımlar: DXF → Taban kenarı (L1) → Reçeteye ekle / CSV·DAT / FTP
- Butonlar: **Dosya Seç**, **CSV İçe Aktar**

`[EKRAN GÖRÜNTÜSÜ: Hoş geldiniz paneli]`

---

## 3. İlk kurulum — Ayarlar

Reçeteye şekil eklemeden veya simülasyon çalıştırmadan önce ayarların yapılması gerekir. Ayarlar eksikse uygulama uyarı verir ve Ayarlar penceresini açar.

1. Üst bardan **Ayarlar**’a tıklayın.
2. Gerekli alanları doldurun.
3. **Kaydet…** ile onaylayın.

`[EKRAN GÖRÜNTÜSÜ: Ayarlar penceresi]`

### 3.1 Ayar alanları

| Alan | Zorunlu | Açıklama |
|------|---------|----------|
| **Dil** | Hayır | Türkçe / English / Deutsch. Kaydedince arayüz anında değişir. |
| **Makine yönü (*)** | Evet | **Soldan sağa** veya **Sağdan sola**. Makinenin işleme yönüne göre seçilir. |
| **Maks. şekil eni (mm) (*)** | Evet | Makinenin kabul ettiği maksimum genişlik. 0’dan büyük olmalı. |
| **Maks. şekil boyu (mm) (*)** | Evet | Makinenin kabul ettiği maksimum yükseklik. 0’dan büyük olmalı. |
| **FTP ayarları…** | İsteğe bağlı | Makineye FTP ile göndermek için bağlantı bilgileri. |

**Not:** Makine yönü değişirse açık DXF yeniden hizalanır.

---

## 4. FTP ayarları

Makineye doğrudan dosya göndermek için:

1. **Ayarlar** → **FTP ayarları…**
2. Bağlantı bilgilerini girin.
3. Kaydedin.

`[EKRAN GÖRÜNTÜSÜ: FTP Ayarları penceresi]`

| Alan | Açıklama | Örnek |
|------|----------|--------|
| **Sunucu (IP / adres)** | Makine veya FTP sunucusu | `192.168.1.50` |
| **Port** | FTP portu (1–65535) | `21` |
| **Kullanıcı adı** | FTP kullanıcı adı | `ftpuser` |
| **Şifre** | FTP şifresi | (gizli yazılır) |
| **Uzak klasör** | Dosyanın yükleneceği klasör | `programs` |

### 4.1 Uzak klasörü görüntüleme

**Uzak klasörü görüntüle…** ile sunucudaki dosyaları listeleyebilir, yenileyebilir veya silebilirsiniz.

`[EKRAN GÖRÜNTÜSÜ: Uzak FTP Dosyaları penceresi]`

---

## 5. DXF dosyası yükleme

1. **Dosya Seç**’e tıklayın (veya Hoş geldiniz ekranından).
2. `*.dxf` dosyasını seçin.
3. Sol alanda çizim, sağ altta şekil analizi görünür.

`[EKRAN GÖRÜNTÜSÜ: DXF yüklenmiş ana ekran]`

### 5.1 Çizimde görünenler

- Kapalı kontur (şekil dış hattı)
- Köşe etiketleri: **K1, K2, K3…**
- Kenar etiketleri: **L1, L2, L3…**
- Menfezler varsa: **M1, M2…**
- Radius / yay bilgileri ve açılar
- L1 başlangıç işareti (taban kenarı belirlendikten sonra)

### 5.2 Durum çubuğu

Altta özet istatistik görünür, örneğin:

> Kontur kenar: n \| Radius: n \| Yay: n \| Daire: n \| Menfez: n \| Entity: n

### 5.3 Boyut sınırı uyarısı

Şekil, Ayarlar’daki **maks. en / boy** değerlerini aşarsa uyarı alırsınız. Bu durumda reçeteye ekleme ve simülasyon kilitlenir. Çizimi veya limitleri gözden geçirin.

---

## 6. Taban kenarı seçimi (L1)

Makine için şeklin hangi kenardan başlayacağı **L1 (taban kenarı)** ile belirlenir.

1. DXF yüklü ve kapalı kontur varken **Taban Kenarı Seç**’e tıklayın.
2. Durum çubuğunda: *Taban olacak kenara tıklayın (L1).*
3. Çizimde istediğiniz kenara tıklayın.
4. Şekil, seçilen kenar L1 olacak şekilde döndürülür / hizalanır.

`[EKRAN GÖRÜNTÜSÜ: Taban kenarı seçimi öncesi / sonrası]`

**İpuçları:**

- Kenara yeterince yakın tıklayın; aksi halde “daha yakın tıklayın” uyarısı çıkabilir.
- Kapalı kontur yoksa buton çalışmaz / uyarı verir.
- Reçeteye eklemeden önce L1’i doğru seçmek önemlidir; kenar sırası buna göre oluşur.

---

## 7. Şekil analizi ve kenar uzunlukları

Sağ alttaki **Şekil analizi** paneli salt okunur bilgilendirme alanıdır.

DXF yüklendikten sonra burada tipik olarak şunlar yer alır:

- Her kenar için başlangıç / bitiş koordinatları
- **Uzunluk: … mm** (kenar uzunluğu)
- Yaylı kenarlarda radius bilgisi
- Menfez merkez, yarıçap, uzaklık
- Radius bükeylik, R değeri, köşe / teğet açıları

`[EKRAN GÖRÜNTÜSÜ: Şekil analizi paneli — kenar uzunlukları]`

### 7.1 Önemli bilgilendirme

| Konu | Açıklama |
|------|----------|
| Analiz paneli | Kullanıcıya gösterilen ölçü özeti |
| CSV’deki **L1…L12** | Program tarafından DXF konturundan **otomatik** hesaplanır |
| Elle L girme | Yeni DXF şekillerinde kenar uzunluğunu elle girmeniz gerekmez |
| Maksimum kenar | Makine satırında en fazla **12 kenar** desteklenir |

---

## 8. Reçete yönetimi

Sağ üstteki **Reçete** listesi, makineye gönderilecek şekilleri tutar.

### 8.1 Liste sütunları

| Sütun | Anlam |
|--------|--------|
| **#** | Sıra numarası |
| **Dosya** | DXF adı veya CSV satır etiketi |
| **Kenar** | Kenar sayısı |
| **Cam kalınlığı** | mm cinsinden cam kalınlığı |
| **Adet** | Üretilecek adet |
| **Kaynak** | **Yeni** = DXF’ten eklendi · **CSV** = dosyadan içe aktarıldı |

`[EKRAN GÖRÜNTÜSÜ: Reçete listesi]`

Liste butonları:

- **Seçileni Düzenle**
- **Seçileni Kaldır**
- **Tümünü Temizle**

Satıra **çift tıklamak** da düzenleme penceresini açar.

---

## 9. Reçeteye şekil ekleme

1. DXF yükleyin.
2. Gerekirse **Taban Kenarı Seç** ile L1’i belirleyin.
3. Ayarların tamamlandığından ve şeklin limit içinde olduğundan emin olun.
4. **Reçeteye Ekle**’ye tıklayın.
5. **Reçete Parametreleri** penceresini doldurun.
6. Onaylayın → listede **Kaynak = Yeni** satır oluşur.

`[EKRAN GÖRÜNTÜSÜ: Reçete Parametreleri penceresi]`

### 9.1 Reçete parametreleri

| Alan | Açıklama | Tipik / kural |
|------|----------|----------------|
| **Kenar kalınlık** (her kenar) | O kenardaki işleme kalınlığı (mm) | Varsayılan 10; 0 = o kenarda işleme yok (rapid) |
| **Offset** (her kenar) | Kenar ofseti (mm) | Varsayılan 0 |
| **Menfez M# kalınlık** | Menfez sıyırma kalınlığı (menfez varsa) | Varsayılan 10 |
| **Taş genişliği** | Takım / taş genişliği (mm) | > 0 olmalı |
| **Bindirme** | Pass’ler arası örtüşme (mm) | ≥ 0 ve taş genişliğinden küçük |
| **Cam kalınlığı** | Cam kalınlığı (mm) | ≥ 1 |
| **İstenilen adet** | Üretilecek adet | ≥ 1 |

**Not:** Köşe / kenar numaralandırması çizimdeki **K** ve **L** etiketleriyle uyumludur. Sıra genellikle CCW (saat yönünün tersi), referans (0,0) üzerindendir. Radius’lar ayrı kenar olarak ele alınır.

---

## 10. Reçete düzenleme

Listeden bir satır seçip **Seçileni Düzenle** (veya çift tık):

### 10.1 Kaynak = Yeni (DXF’ten eklenen)

- Pencere başlığı: **Reçete Şeklini Düzenle**
- Kalınlık, offset, taş, bindirme, cam kalınlığı, adet güncellenir
- Değişiklikler bellekte kalır; kalıcı dosya için **Toplu CSV / DAT** veya **FTP** gerekir

`[EKRAN GÖRÜNTÜSÜ: Reçete Şeklini Düzenle]`

### 10.2 Kaynak = CSV (içe aktarılmış satır)

- Pencere başlığı: **İçe Aktarılmış CSV Satırını Düzenle**
- **Geometri kilitlidir:** L, R, A ve menfez konumları değişmez
- Yalnızca kullanıcı parametreleri (SA / offset / kalınlık / adet vb.) düzenlenir
- Mümkünse değişiklik kaynak CSV dosyasına yazılır

`[EKRAN GÖRÜNTÜSÜ: İçe Aktarılmış CSV Satırını Düzenle]`

---

## 11. Reçeteden satır silme / temizleme

| İşlem | Nasıl | Sonuç |
|--------|--------|--------|
| **Seçileni Kaldır** | Satırı seç → buton → onay | Tek satır silinir |
| **Tümünü Temizle** | Buton → onay | Hem CSV satırları hem yeni şekiller silinir |

Onay pencerelerinde **Evet, devam** / **Hayır** seçenekleri çıkar.

`[EKRAN GÖRÜNTÜSÜ: Silme onay penceresi]`

---

## 12. CSV içe aktarma

Mevcut bir reçete dosyasını açmak için:

1. **CSV İçe Aktar**’a tıklayın.
2. `*.csv` dosyasını seçin.
3. Satırlar reçete listesine **Kaynak = CSV** olarak gelir.

`[EKRAN GÖRÜNTÜSÜ: CSV içe aktarma sonrası liste]`

**Teknik notlar:**

- Ayırıcı: noktalı virgül (`;`)
- Makine satırı sabit alan yapısındadır
- Hatalı / eksik satırlarda uyarı veya hata mesajı gösterilir
- Sonradan DXF’ten eklenen şekiller, dışa aktarımda mevcut CSV satırlarının **ardına** eklenir

### 12.1 Tipik senaryo: mevcut CSV’ye yeni şekil ekleme

1. CSV İçe Aktar
2. Yeni DXF yükle → Taban kenarı → Reçeteye Ekle
3. Toplu CSV Çıktı (eski satırlar + yeni şekiller birlikte)

---

## 13. Toplu çıktı alma (CSV / DAT)

Reçetede en az bir satır varken:

### 13.1 Toplu CSV Çıktı

1. **Toplu CSV Çıktı**’ya tıklayın.
2. Kayıt yerini ve dosya adını seçin (varsayılan örnek: `recete.csv`).
3. Dosya yazılır.

### 13.2 Toplu DAT Çıktı

1. **Toplu DAT Çıktı**’ya tıklayın.
2. Kayıt yerini seçin (varsayılan örnek: `recete.dat`).
3. Dosya yazılır.

`[EKRAN GÖRÜNTÜSÜ: Toplu CSV / DAT kaydetme diyaloğu]`

**Çıktı sonrası davranış:** Liste yeniden içe aktarılmış gibi güncellenir; “Yeni” satırlar CSV kaynağına dönüşür / temizlenir (programın dışa aktarım sonrası liste yenileme mantığı).

### 13.3 CSV’de neler otomatik, neler kullanıcıdan gelir?

| Alan grubu | Kaynak |
|------------|--------|
| Cam kalınlığı, adet | Kullanıcı (reçete parametreleri) |
| **SA1–SA12** | Kenar işleme kalınlıkları (kullanıcı) |
| **L1–L12** | Kenar uzunlukları (**otomatik**, DXF’ten) |
| **R1–R12** | Radius (± bükeylik) (**otomatik**) |
| **A1–A12** | Açılar (**otomatik**) |
| **O1–O12** | Offset (kullanıcı) |
| Menfez (M_SA, M_X, M_Y, M_R) | Kullanıcı + DXF |

Dosya biçimi: UTF-8 (BOM), ayırıcı `;`, en fazla **12 kenar**.

---

## 14. FTP ile makineye gönderme

1. Reçetede veri olsun.
2. **Ayarlar → FTP ayarları** dolu olsun.
3. **FTP ile Gönder**’e tıklayın.
4. **FTP Gönderimi** penceresinde dosya adını girin (otomatik `.csv` uzantısı).
5. **Gönder**.

`[EKRAN GÖRÜNTÜSÜ: FTP Gönderimi penceresi]`

**Ne olur?**

1. Önce masaüstünde yedek alınır: `Masaüstü\otomasyonreceteler\`
2. Ardından FTP sunucusundaki uzak klasöre yüklenir

Dosya adı örneği: `siparis_2026.csv`

---

## 15. Simülasyon (taş işleme doğrulama)

Simülasyon, reçeteye eklemeden veya ekledikten bağımsız olarak tek bir DXF şekli üzerinde işleme planını görsel olarak doğrular.

1. DXF yükleyin (ayarlar ve limitler uygun olsun).
2. İsteğe bağlı: taban kenarını seçin.
3. **Simülasyon**’a tıklayın.
4. Köşe / kenar numarası onayını okuyun (K1, K2… doğru mu?).
5. **Simülasyon Parametreleri**ni doldurun.
6. **Simülasyonu Başlat**.

`[EKRAN GÖRÜNTÜSÜ: Simülasyon Parametreleri / onay adımı]`

### 15.1 Simülasyon penceresi

Başlık örneği: **Taş Simülasyonu — {dosya adı}**

| Kontrol | İşlev |
|---------|--------|
| **▶ Oynat** | Simülasyonu başlatır / devam ettirir |
| **⏸ Durdur** | Duraklatır |
| **Adım** | Tek adım ilerletir |
| **Sıfırla** | Başa alır |
| **Hız** | 1–20 arası hız |
| **CSV Çıktı** | Bu şekil için tek satır CSV |
| **DAT Çıktı** | Bu şekil için tek satır DAT |

- **Sol:** Canlı kontur ve taş konumu
- **Sağ:** Plan logu, kenar geçişleri, bitişte **SİMÜLASYON RAPORU**

`[EKRAN GÖRÜNTÜSÜ: Simülasyon penceresi — oynatma ve rapor]`

### 15.2 Simülasyon mantığı (özet)

- Kontur turları CCW (L1 → Ln)
- Kalınlığı **0** olan kenarlarda taş kalkık ilerler (rapid)
- Rapor: tur sayısı, dolu işleme / rapid mesafesi, tahmini süre

### 15.3 Simülasyon içinden CSV / DAT

1. **CSV Çıktı** veya **DAT Çıktı**
2. **CSV Çıktısı** penceresinde genel kalınlık ve istenilen adet
3. Kaydet diyaloğu

`[EKRAN GÖRÜNTÜSÜ: CSV Çıktısı (simülasyon) penceresi]`

---

## 16. Butonların ne zaman aktif olduğu

| Buton | Ne zaman kullanılabilir |
|--------|-------------------------|
| Dosya Seç | Her zaman (işlem öncesi ayar istenebilir) |
| CSV İçe Aktar | Her zaman |
| Taban Kenarı Seç | Kapalı kontur varken |
| Reçeteye Ekle / Simülasyon | Kontur var + boyut limiti OK + ayarlar tamam |
| Toplu CSV / DAT / FTP | Reçetede satır varken |
| Seçileni Düzenle / Kaldır | Listede seçim varken |
| Tümünü Temizle | Reçetede veri varken |

---

## 17. Önerilen kullanım senaryoları

### Senaryo A — Sıfırdan reçete

1. Ayarlar (dil, makine yönü, maks. en/boy)
2. Dosya Seç (DXF)
3. Şekil analizi / L etiketlerini kontrol edin
4. Taban Kenarı Seç → L1
5. Reçeteye Ekle → parametreler
6. (İsteğe bağlı) başka DXF’ler ekleyin
7. Toplu CSV / DAT veya FTP ile Gönder

### Senaryo B — Mevcut CSV’ye şekil ekleme

1. CSV İçe Aktar
2. Yeni DXF → Taban kenarı → Reçeteye Ekle
3. Toplu CSV Çıktı

### Senaryo C — Sadece doğrulama (simülasyon)

1. DXF yükle
2. Taban kenarı (önerilir)
3. Simülasyon → oynat → raporu incele
4. Gerekirse simülasyondan tek satır CSV/DAT alın

### Senaryo D — Makineye FTP gönderimi

1. FTP ayarlarını kaydedin
2. Reçeteyi hazırlayın
3. FTP ile Gönder → dosya adı → Gönder
4. Gerekirse uzak klasörde dosyaları kontrol edin / silin

---

## 18. Sık karşılaşılan durumlar

| Durum | Ne yapmalısınız |
|--------|------------------|
| Reçeteye Ekle / Simülasyon açılmıyor | Ayarlar’da makine yönü ve maks. en/boy doldurun |
| Boyut aşımı uyarısı | Şekli küçültün veya maks. limitleri artırın (makine kapasitesine uygun) |
| Taban kenarı seçilemiyor | Önce geçerli kapalı konturlu DXF yükleyin; kenara daha yakın tıklayın |
| FTP gönderilemiyor | Sunucu, port, kullanıcı, şifre ve uzak klasörü kontrol edin; ağ bağlantısını doğrulayın |
| Kenar uzunluğu yanlış görünüyor | Önce L1 (taban kenarı) doğru mu bakın; DXF konturunun kapalı ve temiz olduğundan emin olun |
| 12’den fazla kenar | Desteklenmez; çizimi sadeleştirin veya parçalayın |
| Dil değişmiyor | Ayarlar → Dil → Kaydet… |

---

## 19. Dosya ve klasör konumları

| Ne | Nerede |
|----|--------|
| Uygulama ayarları | `%LocalAppData%\otomasyon\settings.json` |
| Uygulama logu | `%LocalAppData%\otomasyon\app.log` |
| FTP öncesi yerel yedek | `Masaüstü\otomasyonreceteler\` |

---

## 20. Ekran görüntüsü kontrol listesi (manuel yazar için)

Kılavuza eklemeniz önerilen görseller:

1. Hoş geldiniz / boş ana ekran
2. Ayarlar penceresi
3. FTP Ayarları
4. Uzak FTP Dosyaları
5. DXF yüklenmiş ana ekran (L/K etiketleri görünür)
6. Taban kenarı seçimi (önce–sonra)
7. Şekil analizi paneli (kenar uzunlukları)
8. Reçete Parametreleri
9. Reçete listesi (Yeni / CSV satırları)
10. Reçete Şeklini Düzenle
11. İçe Aktarılmış CSV Satırını Düzenle
12. Toplu CSV / DAT kaydetme
13. FTP Gönderimi
14. Simülasyon onay / parametreleri
15. Simülasyon oynatma + rapor
16. CSV Çıktısı (simülasyon)

---

## 21. Kısa referans — menü / buton sözlüğü

| Arayüz metni | Görev |
|--------------|--------|
| Dosya Seç | DXF açar |
| CSV İçe Aktar | Mevcut reçete CSV’sini yükler |
| Taban Kenarı Seç | L1 kenarını belirler |
| Reçeteye Ekle | Açık DXF’i reçeteye parametrelerle ekler |
| Simülasyon | Taş işleme simülasyonu |
| Toplu CSV Çıktı | Tüm reçeteyi CSV kaydeder |
| Toplu DAT Çıktı | Tüm reçeteyi DAT kaydeder |
| FTP ile Gönder | Reçeteyi makineye yükler (önce yerel yedek) |
| Ayarlar | Dil, makine yönü, limitler, FTP |
| Seçileni Düzenle | Satır parametrelerini değiştirir |
| Seçileni Kaldır | Satırı siler |
| Tümünü Temizle | Reçeteyi boşaltır |

---

*Belge, uygulamanın güncel arayüz etiketlerine (`Strings.json`) göre hazırlanmıştır. Sürüm güncellemelerinde buton adları veya alanlar değişirse bu kılavuzu aynı başlıklarla güncelleyin.*
