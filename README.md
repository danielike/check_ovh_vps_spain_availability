## Requirements

* .NET 8, 9 or 10 SDK.

## Step by step

```bash
touch .env
```

Add the following and save:

`TELEGRAM_BOT_TOKEN=yourToken`

```bash
dotnet tool install --global dotnet-script
```

```bash
dotnet script check_ovh_vps_spain_availability.csx
```

or, if you are in OSX/Linux:

```bash
./check_ovh_vps_spain_availability.csx
```
