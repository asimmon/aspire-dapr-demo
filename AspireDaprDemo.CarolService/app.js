import express from "express";
import bodyParser from "body-parser";

const port = process.env.PORT ?? 3000;

const app = express();

app.use(bodyParser.json({ type: "application/*+json" }));

app.get("/dapr/subscribe", (_req, res) => {
  res.json([
    {
      pubsubname: "pubsub",
      topic: "weather",
      route: "subscriptions/weather"
    }
  ]);
});

// We should receive this message only when a fresh weather forecast is produced by the service "bob"
app.post("/subscriptions/weather", (req, res) => {
  console.log("Weather forecast message received:", req.body.data);
  res.sendStatus(200);
});

app.listen(port, () => {
  console.log(`Carol service listening at http://localhost:${port}`);
});
