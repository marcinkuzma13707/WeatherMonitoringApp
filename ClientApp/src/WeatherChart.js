import React, { useEffect, useState } from 'react';
import { Chart, CategoryScale, LinearScale, PointElement, LineElement } from 'chart.js';
import { Line } from 'react-chartjs-2';
import axios from 'axios';
import './WeatherChart.css';

Chart.register(CategoryScale, LinearScale, PointElement, LineElement);

const WeatherChart = () => {
  const [tempData, setTempData] = useState({
    labels: [],
    datasets: [{
      label: 'Min Temperature (°C)',
      data: [],
      borderColor: 'rgba(75,192,192,1)',
      fill: false,
    }]
  });
  const [windData, setWindData] = useState({
    labels: [],
    datasets: [{
      label: 'Max Wind Speed (km/h)',
      data: [],
      borderColor: 'rgba(255,99,132,1)',
      fill: false,
    }]
  });
  const [originalData, setOriginalData] = useState({ tempData: null, windData: null });
  const [loading, setLoading] = useState(true);
  const [showingTrend, setShowingTrend] = useState(false);
  const [currentCity, setCurrentCity] = useState('');

  useEffect(() => {
    const fetchWeatherData = async () => {
      try {
        const response = await axios.get('/api/weather/weather-data');
  
        const citiesWithUpdate = response.data.map(item => `${item.country}/${item.city} (Last updated: ${new Date(item.lastUpdate).toLocaleString()})`);
        const temperatures = response.data.map(item => item.temperature);
        const windSpeeds = response.data.map(item => item.windSpeed);
  
        setTempData({
          labels: citiesWithUpdate,
          datasets: [{
            label: 'Min Temperature (°C)',
            data: temperatures,
            borderColor: 'rgba(75,192,192,1)',
            fill: false,
          }]
        });
  
        setWindData({
          labels: citiesWithUpdate,
          datasets: [{
            label: 'Max Wind Speed (km/h)',
            data: windSpeeds,
            borderColor: 'rgba(255,99,132,1)',
            fill: false,
          }]
        });
  
        setOriginalData({
          tempData: {
            labels: citiesWithUpdate,
            datasets: [{
              label: 'Min Temperature (°C)',
              data: temperatures,
              borderColor: 'rgba(75,192,192,1)',
              fill: false,
            }]
          },
          windData: {
            labels: citiesWithUpdate,
            datasets: [{
              label: 'Max Wind Speed (km/h)',
              data: windSpeeds,
              borderColor: 'rgba(255,99,132,1)',
              fill: false,
            }]
          }
        });
  
        setLoading(false);
      } catch (error) {
        console.error("Error fetching weather data", error);
        setLoading(false);
      }
    };
  
    fetchWeatherData();
  }, []);
  
  const handleTrendClick = async (cityWithCountry) => {
    const [countryCity, lastUpdated] = cityWithCountry.split(' (Last updated:'); 
    const [country, city] = countryCity.split('/');
  
    setCurrentCity(`${city}/${country}`);  
    try {
      const trendResponse = await axios.get('/api/weather/weather-trend', {
        params: { city, hours: 2 }
      });
  
      if (!trendResponse.data || trendResponse.data.length === 0) {
        console.error("No data returned from the trend API.");
        return;
      }
  
      const trendLabels = trendResponse.data.map(item => new Date(item.lastUpdate).toLocaleTimeString());
      const tempTrends = trendResponse.data.map(item => item.temperature);
      const windTrends = trendResponse.data.map(item => item.windSpeed);
  
      setTempData({
        labels: trendLabels,
        datasets: [{
          label: `Temperature Trend (°C) for ${city}, ${country}`,
          data: tempTrends,
          borderColor: 'rgba(54, 162, 235, 1)',
          fill: false,
        }]
      });
  
      setWindData({
        labels: trendLabels,
        datasets: [{
          label: `Wind Speed Trend (km/h) for ${city}, ${country}`,
          data: windTrends,
          borderColor: 'rgba(255, 99, 132, 1)',
          fill: false,
        }]
      });
  
      setShowingTrend(true);
    } catch (error) {
      console.error("Error fetching trend data", error);
    }
  };
  
  
  
  const handleBackClick = () => {
    setTempData(originalData.tempData);
    setWindData(originalData.windData);
    setShowingTrend(false);
    setCurrentCity('');
  };

  if (loading) {
    return <div>Loading...</div>;
  }

  return (
<div className="weather-chart-container">
  <h2 className="title-small">Weather Monitoring</h2>
  {!showingTrend && (
    <p>Click on a point on the graph to see the trend for the last 2 hours.</p>
  )}
  {showingTrend && (
    <button className="styled-button" onClick={handleBackClick}>
      Back to Summary
    </button>
  )}

  <div className="chart-wrapper">
    <div>
      <div className="chart-title">
        {showingTrend
          ? `Temperature Trend (°C) for ${currentCity}`
          : "Min Temperature (°C)"}
      </div>
      <div className="chart-container">
        <Line
          key={JSON.stringify(tempData)}
          data={tempData}
          options={{
            maintainAspectRatio: false,
            responsive: true,
            onClick: (e) => {
              const elements = e.chart.getElementsAtEventForMode(
                e.native,
                "nearest",
                { intersect: true },
                false
              );
              if (elements.length > 0) {
                const index = elements[0].index;
                const city = originalData.tempData.labels[index];
                handleTrendClick(city);
              }
            },
          }}
        />
      </div>
    </div>
    <div>
      <div className="chart-title">
        {showingTrend
          ? `Wind Speed Trend (km/h) for ${currentCity}`
          : "Max Wind Speed (km/h)"}
      </div>
      <div className="chart-container">
        <Line
          key={JSON.stringify(windData)}
          data={windData}
          options={{
            maintainAspectRatio: false,
            responsive: true,
            onClick: (e) => {
              const elements = e.chart.getElementsAtEventForMode(
                e.native,
                "nearest",
                { intersect: true },
                false
              );
              if (elements.length > 0) {
                const index = elements[0].index;
                const city = originalData.windData.labels[index];
                handleTrendClick(city);
              }
            },
          }}
        />
      </div>
    </div>
  </div>
</div>
  );
};

export default WeatherChart;

